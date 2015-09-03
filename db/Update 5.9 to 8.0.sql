-- ------------------------------------------------------------------------------------------
-- Copyright AspDotNetStorefront.com.  All Rights Reserved.
-- http://www.aspdotnetstorefront.com
-- For details on this license please visit our homepage at the URL above.
-- THE ABOVE NOTICE MUST REMAIN INTACT. 
--
-- Database Creation Script: AspDotNetStorefront Version ML 8.0, Microsoft SQL Server 2005 Or higher
-- ------------------------------------------------------------------------------------------

/***** ASPDOTNETSTOREFRONT v5.9 to v8.0 DB update [dbo].SCRIPT   ******/
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

--Script level variables
declare @cmd varchar(8000), @defaultname varchar(800)

exec aspdnsf_RemoveDuplicateAppConfigs;
go

exec aspdnsf_RemoveDuplicateAppConfigs;
go

exec aspdnsf_RemoveDuplicateAppConfigs;
go




-- reset any new tables, views, procs, etc to db owned by dbo:
declare @cmd varchar(4000), @objname varchar(255), @username varchar(255)
select distinct s.name objname, u.name username into #tmp from [dbo].sysobjects s join [dbo].sysusers u on s.uid = u.uid 
where u.name <> 'dbo'and xtype in ('V', 'P', 'U') and u.name not like 'INFORMATION%' 

select top 1 @objname = ltrim(objname), @username = ltrim(username) from #tmp
while @@rowcount > 0 begin
set @cmd = 'sp_changeobjectowner @objname = ''' + @username + '.' + @objname + ''' , @newowner = ''dbo''' 
exec(@cmd)

delete #tmp where objname= @objname and username = @username 
select top 1 @objname = ltrim(objname), @username = ltrim(username) from #tmp
end
drop table #tmp







if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_DropColumnWithDefaultConstraint]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
    DROP PROCEDURE [dbo].[aspdnsf_DropColumnWithDefaultConstraint]
GO
CREATE PROC [dbo].[aspdnsf_DropColumnWithDefaultConstraint]
  @TableName varchar(100), 
  @ColumnName varchar(100) 
WITH ENCRYPTION   
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
 

/**********************************  table mods and creates  **********************************/


	if (not exists (select * From syscolumns where id = object_id('ProductView') and name = 'CustomerViewID')
		and exists (select * From sysobjects where id = object_id('ProductView') and type = 'u'))
	begin
		drop table [dbo].ProductView
	end
	go
	
	 if not exists (select * From sysobjects where id = object_id('ProductView') and type = 'u') begin
		CREATE TABLE [dbo].[ProductView](
		[ViewID] [int] IDENTITY(1,1) NOT NULL,
		[CustomerViewID] NVARCHAR(50)  NOT NULL,
		[ProductID] [int] NOT NULL,
		[ViewDate] [datetime] NOT NULL,
		CONSTRAINT [PK_ProductView] PRIMARY KEY CLUSTERED 
		(
		[ViewID] ASC
		)
		)
	end
	GO




    if not exists (select * From sysobjects where id = object_id('ShippingImportExport') and type = 'u') begin
        CREATE TABLE [dbo].[ShippingImportExport](
            [OrderNumber] [int] NOT NULL,
            [CustomerID] [int] NOT NULL,
            [CompanyName] [nvarchar](50) NULL,
            [CustomerLastName] [nvarchar](50) NOT NULL,
            [CustomerFirstName] [nvarchar](50) NOT NULL,
            [CustomerPhone] [nvarchar](50) NULL,
            [CustomerEmail] [nvarchar](100) NULL,
            [Address1] [nvarchar](100) NOT NULL,
            [Address2] [nvarchar](100) NULL,
            [Suite] [nvarchar](50) NULL,
            [City] [nvarchar](100) NOT NULL,
            [State] [nvarchar](100) NOT NULL,
            [Zip] [nvarchar](10) NOT NULL,
            [Country] [nvarchar](100) NOT NULL,
            [ServiceCarrierCode] [nvarchar](50) NULL,
            [TrackingNumber] [nvarchar](100) NULL,
            [Cost] [money] NULL,
            [Weight] [money] NULL,
            CONSTRAINT [PK_ShippingImportExport] PRIMARY KEY CLUSTERED
            (
                [OrderNumber] ASC
            )
        )
    end

    GO


    if not exists (select * From sysobjects where id = object_id('AuditLog') and type = 'u') begin
        CREATE TABLE [dbo].[AuditLog](
            [AuditLogID] [bigint] IDENTITY(1,1) NOT NULL,
            [ActionDate] [datetime] NOT NULL CONSTRAINT [DF_AuditLog_ActionDate]  DEFAULT (getdate()),
            [CustomerID] [int] NOT NULL,
            [UpdatedCustomerID] [int] NOT NULL,
            [OrderNumber] [int] NOT NULL,
            [Description] [nvarchar](100) NOT NULL,
            [Details] [nvarchar](1000) NOT NULL,
            [PagePath] [nvarchar](200) NOT NULL,
            [AuditGroup] [nvarchar](30) NOT NULL
            CONSTRAINT [PK_AuditLog] PRIMARY KEY CLUSTERED 
            (
                [AuditLogID] ASC
            )
        )
    end
    GO




    if not exists (select * From sysobjects where id = object_id('EventHandler') and type = 'u') begin
        CREATE TABLE [dbo].[EventHandler]
        (
            EventID int not null IDENTITY,
            EventName nvarchar(20) not null,
            CalloutURL varchar(200) not null,
            XmlPackage varchar(100) not null,
            Debug bit not null CONSTRAINT DF_EventHandler_Debug DEFAULT (0),
            Active bit not null CONSTRAINT DF_EventHandler_Active DEFAULT (0),
            CONSTRAINT PK_EventHandler PRIMARY KEY CLUSTERED 
            (
                EventID 
            )
        )
    end
    GO





    IF not exists (SELECT * FROM sysobjects where name = 'TaxClass' and type = 'u')
        CREATE TABLE [dbo].[TaxClass](
            [TaxClassID] [int] IDENTITY(1,1) NOT NULL,
            [TaxClassGUID] [uniqueidentifier] NOT NULL CONSTRAINT DF_TaxClass_TaxClassGUID DEFAULT(newid()),
            [Name] [nvarchar](400) NOT NULL,
            [TaxCode] [nvarchar](100) NOT NULL,
            [DisplayOrder] [int] NOT NULL CONSTRAINT DF_TaxClass_DisplayOrder DEFAULT((1)),
            [CreatedOn] [datetime] NOT NULL CONSTRAINT DF_TaxClass_CreatedOn DEFAULT(getdate()),
            CONSTRAINT [PK_TaxClass] PRIMARY KEY CLUSTERED 
            (
                [TaxClassID] ASC
            )
        )
    GO

    IF not exists (SELECT * FROM sysobjects where name = 'CustomReport' and type = 'u')
        CREATE TABLE [dbo].[CustomReport](
            [CustomReportID] [int] IDENTITY(1,1) NOT NULL,
            [CustomReportGUID] [uniqueidentifier] NOT NULL CONSTRAINT DF_CustomReport_CustomReportGUID DEFAULT (newid()),
            [Name] [nvarchar](400) NOT NULL,
            [Description] [ntext] NULL,
            [SQLCommand] [ntext] NULL,
            [CreatedOn] [datetime] NOT NULL CONSTRAINT DF_CustomReport_CreatedOn DEFAULT (getdate()),
             CONSTRAINT [PK_CustomReport] PRIMARY KEY CLUSTERED 
            (
                [CustomReportID] ASC
            )
        )
    GO

    IF not exists (SELECT * FROM sysobjects where name = 'ShippingByTotalByPercent' and type = 'u')
        CREATE TABLE [dbo].[ShippingByTotalByPercent](
            [RowGUID] [uniqueidentifier] NOT NULL CONSTRAINT DF_ShippingByTotalByPercent_RowGUID DEFAULT(newid()),
            [LowValue] [money] NOT NULL,
            [HighValue] [money] NOT NULL,
            [ShippingMethodID] [int] NOT NULL,
            [MinimumCharge] [money] NOT NULL,
            [SurCharge] [money] NOT NULL,
            [PercentOfTotal] [money] NOT NULL,
            [CreatedOn] [datetime] NOT NULL CONSTRAINT DF_ShippingByTotalByPercent_CreatedOn DEFAULT(getdate())
        )
    GO


    IF not exists (SELECT * FROM sysobjects where name = 'PasswordLog' and type = 'u') BEGIN
        create table dbo.PasswordLog 
        (
            CustomerID int not null,
            ChangeDt datetime not null,
            OldPwd nvarchar(250) not null
        )

        create clustered index CIX_PasswordLog on dbo.PasswordLog (CustomerID, ChangeDt)
    END
    go



    if not exists (SELECT * FROM sysobjects where name = 'SecurityLog' and type = 'u') begin
        CREATE TABLE [dbo].[SecurityLog](
            [logid] [bigint] IDENTITY(1,1) NOT NULL,
            [SecurityAction] [nvarchar](1000) NOT NULL, -- encrypted
            [Description] [ntext] NOT NULL, -- encrypted
            [ActionDate] [datetime] NOT NULL CONSTRAINT [DF_SecurityLog_ActionDate]  DEFAULT (getdate()),
            [CustomerUpdated] [int] NOT NULL,
            [UpdatedBy] [int] NOT NULL,
            [CustomerSessionID] [int] NOT NULL,
            CONSTRAINT [PK_SecurityLog] PRIMARY KEY CLUSTERED 
            (
                [logid] ASC
            )
        )
    end
    go


    if exists (select * From sysobjects where id = object_id('customersession') )
        DROP TABLE [dbo].[CustomerSession]
    GO
        CREATE TABLE [dbo].[CustomerSession](
            [CustomerSessionID] [int] IDENTITY(1,1) NOT NULL,
            [CustomerID] [int] NOT NULL,
            [SessionName] [nvarchar](100) NOT NULL,
            [SessionValue] [ntext] NULL,
            [ipaddr] varchar(15) NULL, 
            [LastActivity] datetime NOT NULL CONSTRAINT DF_customersession_LastActivity default (getdate()),
            [LoggedOut] datetime NULL,
            [CustomerSessionGUID] uniqueidentifier NOT NULL CONSTRAINT DF_customersession_CustomerSessionGUID default(newid()),
            [CreatedOn] [datetime] NOT NULL CONSTRAINT DF_CustomerSession_CreatedOn DEFAULT(getdate()),
            [ExpiresOn] [datetime] NOT NULL CONSTRAINT DF_CustomerSession_ExpiresOn DEFAULT (dateadd(month,(1),getdate())),
            CONSTRAINT [PK_CustomerSession] PRIMARY KEY CLUSTERED 
            (
                [CustomerSessionID] ASC
            )
        )
    go

    if not exists (SELECT * FROM sysobjects where name = 'RestrictedIP' and type = 'u') begin
        CREATE TABLE [dbo].[RestrictedIP](
            [DBRecNo] [int] IDENTITY(1,1) NOT NULL,
            [IPAddress] [nvarchar] (25) NOT NULL,
            [CreatedOn] [datetime] NOT NULL CONSTRAINT DF_RestrictedIP_CreatedOn DEFAULT (getdate()),
             CONSTRAINT [PK_IPAddress] PRIMARY KEY CLUSTERED 
            (
                [IPAddress] ASC
            )
        )
    end
    GO




    if not exists (select * From sysobjects where id = object_id('ProductGenre') and type = 'u') begin
        CREATE TABLE [dbo].[ProductGenre](
            [ProductID] [int] NOT NULL,
            [GenreID] [int] NOT NULL,
            [DisplayOrder] [int] NOT NULL CONSTRAINT DF_ProductGenre_DisplayOrder DEFAULT((1)),
            [CreatedOn] [datetime] NOT NULL CONSTRAINT DF_ProductGenre_CreatedOn DEFAULT(getdate()),
            CONSTRAINT [PK_ProductGenre] PRIMARY KEY CLUSTERED 
            (
                [ProductID] ASC,
                [GenreID] ASC
            )
        )

    end

    GO

    if not exists (select * From sysobjects where id = object_id('Genre') and type = 'u') begin

        CREATE TABLE [dbo].[Genre](
            [GenreID] [int] IDENTITY(1,1) NOT NULL,
            [GenreGUID] [uniqueidentifier] NOT NULL CONSTRAINT DF_Genre_GenreGUID DEFAULT(newid()),
            [Name] [nvarchar](400) NOT NULL,
            [SEName] [nvarchar](100) NULL,
            [SEKeywords] [ntext] NULL,
            [SEDescription] [ntext] NULL,
            [SETitle] [ntext] NULL,
            [SENoScript] [ntext] NULL,
            [SEAltText] [ntext] NULL,
            [Address1] [nvarchar](100) NULL,
            [Address2] [nvarchar](100) NULL,
            [Suite] [nvarchar](25) NULL,
            [City] [nvarchar](100) NULL,
            [State] [nvarchar](100) NULL,
            [ZipCode] [nvarchar](10) NULL,
            [Country] [nvarchar](100) NULL,
            [Phone] [nvarchar](25) NULL,
            [FAX] [nvarchar](25) NULL,
            [URL] [nvarchar](255) NULL,
            [Email] [nvarchar](100) NULL,
            [Summary] [ntext] NULL,
            [Description] [ntext] NULL,
            [Notes] [ntext] NULL,
            [QuantityDiscountID] [int] NULL,
            [SortByLooks] [tinyint] NOT NULL CONSTRAINT DF_Genre_SortByLooks DEFAULT((0)),
            [XmlPackage] [nvarchar](100) NULL,
            [ColWidth] [int] NOT NULL CONSTRAINT DF_Genre_ColWidth DEFAULT((4)),
            [DisplayOrder] [int] NOT NULL CONSTRAINT DF_Genre_DisplayOrder DEFAULT((1)),
            [ExtensionData] [ntext] NULL,
            [ContentsBGColor] [nvarchar](10) NULL,
            [PageBGColor] [nvarchar](10) NULL,
            [GraphicsColor] [nvarchar](20) NULL,
            [NotificationXmlPackage] [nvarchar](100) NULL,
            [ImageFilenameOverride] [ntext] NULL,
            [ParentGenreID] [int] NOT NULL CONSTRAINT DF_Genre_ParentGenreID DEFAULT((0)),
            [Published] [tinyint] NOT NULL CONSTRAINT DF_Genre_Published DEFAULT((1)),
            [Wholesale] [tinyint] NOT NULL CONSTRAINT DF_Genre_Wholesale DEFAULT((0)),
            [IsImport] [tinyint] NOT NULL CONSTRAINT DF_Genre_IsImport DEFAULT((0)),
            [Deleted] [tinyint] NOT NULL CONSTRAINT DF_Genre_Deleted DEFAULT((0)),
            [CreatedOn] [datetime] NOT NULL CONSTRAINT DF_Genre_CreatedOn DEFAULT(getdate()),
            [PageSize] [int] NOT NULL CONSTRAINT DF_Genre_PageSize DEFAULT((20)),
            [TaxClassID] [int] NOT NULL CONSTRAINT DF_Genre_TaxClassID DEFAULT((1)), -- this field is not used, but is here for uniformity as an Entity table
            CONSTRAINT [PK_Genre] PRIMARY KEY CLUSTERED 
            (
                [GenreID] ASC
            )
        )

    end

    GO


    if not exists (select * From sysobjects where id = object_id('ProductVector') and type = 'u') begin

        CREATE TABLE [dbo].[ProductVector](
            [ProductID] [int] NOT NULL,
            [VectorID] [int] NOT NULL,
            [DisplayOrder] [int] NOT NULL CONSTRAINT DF_ProductVector_DisplayOrder DEFAULT((1)),
            [CreatedOn] [datetime] NOT NULL CONSTRAINT DF_ProductVector_CreatedOn DEFAULT(getdate()),
            CONSTRAINT [PK_ProductVector] PRIMARY KEY CLUSTERED 
            (
                [ProductID] ASC,
                [VectorID] ASC
            )
        )

    end

    GO

    if not exists (select * From sysobjects where id = object_id('Vector') and type = 'u') begin

        CREATE TABLE [dbo].[Vector](
            [VectorID] [int] IDENTITY(1,1) NOT NULL,
            [VectorGUID] [uniqueidentifier] NOT NULL CONSTRAINT DF_Vector_VectorGUID DEFAULT(newid()),
            [Name] [nvarchar](400) NOT NULL,
            [SEName] [nvarchar](100) NULL,
            [SEKeywords] [ntext] NULL,
            [SEDescription] [ntext] NULL,
            [SETitle] [ntext] NULL,
            [SENoScript] [ntext] NULL,
            [SEAltText] [ntext] NULL,
            [Address1] [nvarchar](100) NULL,
            [Address2] [nvarchar](100) NULL,
            [Suite] [nvarchar](25) NULL,
            [City] [nvarchar](100) NULL,
            [State] [nvarchar](100) NULL,
            [ZipCode] [nvarchar](10) NULL,
            [Country] [nvarchar](100) NULL,
            [Phone] [nvarchar](25) NULL,
            [FAX] [nvarchar](25) NULL,
            [URL] [nvarchar](255) NULL,
            [Email] [nvarchar](100) NULL,
            [Summary] [ntext] NULL,
            [Description] [ntext] NULL,
            [Notes] [ntext] NULL,
            [QuantityDiscountID] [int] NULL,
            [SortByLooks] [tinyint] NOT NULL CONSTRAINT DF_Vector_SortByLooks DEFAULT((0)),
            [XmlPackage] [nvarchar](100) NULL,
            [ColWidth] [int] NOT NULL CONSTRAINT DF_Vector_ColWidth DEFAULT((4)),
            [DisplayOrder] [int] NOT NULL CONSTRAINT DF_Vector_DisplayOrder DEFAULT((1)),
            [ExtensionData] [ntext] NULL,
            [ContentsBGColor] [nvarchar](10) NULL,
            [PageBGColor] [nvarchar](10) NULL,
            [GraphicsColor] [nvarchar](20) NULL,
            [NotificationXmlPackage] [nvarchar](100) NULL,
            [ImageFilenameOverride] [ntext] NULL,
            [ParentVectorID] [int] NOT NULL CONSTRAINT DF_Vector_ParentVectorID DEFAULT((0)),
            [Published] [tinyint] NOT NULL CONSTRAINT DF_Vector_Published DEFAULT((1)),
            [Wholesale] [tinyint] NOT NULL CONSTRAINT DF_Vector_Wholesale DEFAULT((0)),
            [IsImport] [tinyint] NOT NULL CONSTRAINT DF_Vector_IsImport DEFAULT((0)),
            [Deleted] [tinyint] NOT NULL CONSTRAINT DF_Vector_Deleted DEFAULT((0)),
            [CreatedOn] [datetime] NOT NULL CONSTRAINT DF_Vector_CreatedOn DEFAULT(getdate()),
            [PageSize] [int] NOT NULL CONSTRAINT DF_Vector_PageSize DEFAULT((20)),
            [TaxClassID] [int] NOT NULL CONSTRAINT DF_Vector_TaxClassID DEFAULT((1)), -- this field is not used, but is here for uniformity as an Entity table
            CONSTRAINT [PK_Vector] PRIMARY KEY CLUSTERED 
            (
                [VectorID] ASC
            )
        )
    end

    GO


    IF exists (SELECT * FROM sysobjects where name = 'StateTaxRate' and type = 'u') and not exists (select * from syscolumns where id = object_id('StateTaxRate') and name = 'StateID')
        DROP TABLE StateTaxRate
    go

    IF not exists (SELECT * FROM sysobjects where name = 'StateTaxRate' and type = 'u') BEGIN
        CREATE TABLE [dbo].[StateTaxRate](
            [StateTaxID] int NOT NULL IDENTITY,
            [StateID] [int] NOT NULL,
            [TaxClassID] [int] NOT NULL CONSTRAINT DF_StateTaxRate_TaxClassID DEFAULT((1)),
            [TaxRate] [money] NULL,
            [CreatedOn] [datetime] NOT NULL CONSTRAINT DF_StateTaxRate_CreatedOn DEFAULT(getdate()),
            CONSTRAINT [PK_StateTaxRate] PRIMARY KEY CLUSTERED 
            (
                [StateTaxID] ASC
            )
        )
        CREATE UNIQUE INDEX [UIX_StateTaxRate_StateID_TaxClassID] ON [StateTaxRate] (StateID,TaxClassID) 
    END
    GO



    IF not exists (SELECT * FROM sysobjects where name = 'CountryTaxRate' and type = 'u') BEGIN
        CREATE TABLE [dbo].[CountryTaxRate](
            [CountryTaxID] int NOT NULL IDENTITY,
            [CountryID] [int] NOT NULL,
            [TaxClassID] [int] NOT NULL CONSTRAINT DF_CountryTaxRate_TaxClassID DEFAULT((1)),
            [TaxRate] [money] NULL,
            [CreatedOn] [datetime] NOT NULL CONSTRAINT DF_CountryTaxRate_CreatedOn DEFAULT(getdate()),
            CONSTRAINT [PK_CountryTaxRate] PRIMARY KEY CLUSTERED 
            (
                [CountryTaxID] ASC
            )
        )
        CREATE UNIQUE INDEX [UIX_CountryTaxRate_CountryID_TaxClassID] ON [CountryTaxRate] (CountryID,TaxClassID)
    END
    GO


    if not exists (select * From sysobjects where id = object_id('Feed') and type = 'u') begin
        CREATE TABLE [dbo].[Feed](
            [FeedID] [int] IDENTITY(1,1) NOT NULL,
            [FeedGUID] [uniqueidentifier] NOT NULL CONSTRAINT DF_Feed_FeedGUID DEFAULT(newid()),
            [Name] [nvarchar](100) NULL,
            [DisplayOrder] [int] NOT NULL CONSTRAINT DF_Feed_DisplayOrder DEFAULT((1)),
            [XmlPackage] [nvarchar](100) NULL,
            [CanAutoFTP] [tinyint] NOT NULL CONSTRAINT DF_Feed_CanAutoFTP DEFAULT((0)),
            [FTPUsername] [nvarchar](100) NULL,
            [FTPPassword] [nvarchar](100) NULL,   -- NOT hashed or encrypted, it is not a critical password of any kind!
            [FTPServer] [nvarchar](100) NULL,
            [FTPPort] int NULL,
            [FTPFilename] [nvarchar](1000) NULL,
            [ExtensionData] [ntext] NULL,
            [CreatedOn] [datetime] NOT NULL CONSTRAINT DF_Feed_CreatedOn DEFAULT(getdate()),
            CONSTRAINT [PK_Feed] PRIMARY KEY CLUSTERED 
            (
                [FeedID] ASC
            )
        )
    end
    GO

    if exists (select * From sysobjects where id = object_id('GiftCard') and type = 'u') begin
        drop table [dbo].[GiftCard]
    end
    go
    CREATE TABLE [dbo].[GiftCard](
        [GiftCardID] [int] IDENTITY(1,1) NOT NULL,
        [GiftCardGUID] [uniqueidentifier] NOT NULL CONSTRAINT DF_GiftCard_GiftCardGUID DEFAULT(newid()),
        [SerialNumber] [nvarchar](100) NULL,
        [PurchasedByCustomerID] [int] NOT NULL,
        [OrderNumber] [int] NOT NULL CONSTRAINT DF_GiftCard_OrderNumber DEFAULT((0)),
        [ShoppingCartRecID] [int] NOT NULL CONSTRAINT DF_GiftCard_ShoppingCartRecID DEFAULT((0)),
        [ProductID] [int] NOT NULL CONSTRAINT DF_GiftCard_ProductID DEFAULT((0)), -- not used currently, for future expansion
        [VariantID] [int] NOT NULL CONSTRAINT DF_GiftCard_VariantID DEFAULT((0)), -- not used currently, for future expansion
        [InitialAmount] [money] NOT NULL CONSTRAINT DF_GiftCard_InitialAmount DEFAULT((0.0)),
        [Balance] [money] NOT NULL CONSTRAINT DF_GiftCard_Balance DEFAULT((0.0)),
        [ExpirationDate] [datetime]  CONSTRAINT [DF_GiftCard_ExpirationDate] default dateadd(yy,1,getdate()) NOT NULL,
        [GiftCardTypeID] [int] NULL CONSTRAINT DF_GiftCard_GiftCardTypeID DEFAULT((101)),
        [EMailName] [nvarchar](100) NULL,
        [EMailTo] [nvarchar](100) NULL,
        [EMailMessage] [ntext] NULL,
        [ValidForCustomers] [ntext] NULL, -- not used currently, for future expansion
        [ValidForProducts] [ntext] NULL, -- not used currently, for future expansion
        [ValidForManufacturers] [ntext] NULL, -- not used currently, for future expansion
        [ValidForCategories] [ntext] NULL, -- not used currently, for future expansion
        [ValidForSections] [ntext] NULL, -- not used currently, for future expansion
        [DisabledByAdministrator] [tinyint]  CONSTRAINT [DF_GiftCard_DisabledByAdministrator] default(0) NOT NULL,
        [ExtensionData] [ntext] NULL, -- not used currently, for future expansion
        [CreatedOn] [datetime] NOT NULL CONSTRAINT DF_GiftCard_CreatedOn DEFAULT(getdate()),
        CONSTRAINT [PK_GiftCard] PRIMARY KEY CLUSTERED 
        (
            [GiftCardID] ASC
        )
    )


    GO



    if exists (select * From sysobjects where id = object_id('GiftCardUsage') and type = 'u') begin
        drop table [dbo].[GiftCardUsage]
    end
    go
    CREATE TABLE [dbo].[GiftCardUsage](
        [GiftCardUsageID] [int] IDENTITY(1,1) NOT NULL,
        [GiftCardUsageGUID] [uniqueidentifier] NOT NULL CONSTRAINT DF_GiftCardUsage_GiftCardUsageGUID DEFAULT(newid()),
        [GiftCardID] [int] NOT NULL,
        [UsageTypeID] [int] NOT NULL  CONSTRAINT [DF_GiftCardUsage_UsageTypeID] default(0), -- 0 = used by customer, 1 = funds added by store admin, 2 = funds decremented by store admin
        [UsedByCustomerID] [int] NOT NULL CONSTRAINT DF_GiftCardUsage_UsedByCustomerID DEFAULT((0)),
        [OrderNumber] [int] NOT NULL CONSTRAINT DF_GiftCardUsage_OrderNumber DEFAULT((0)),
        [Amount] [money] NOT NULL CONSTRAINT DF_GiftCardUsage_Amount DEFAULT((0.0)),
        [ExtensionData] [ntext] NULL, -- not used currently, for future expansion
        [CreatedOn] [datetime] NOT NULL CONSTRAINT DF_GiftCardUsage_CreatedOn DEFAULT(getdate()),
        CONSTRAINT [PK_GiftCardUsage] PRIMARY KEY CLUSTERED 
        (
            [GiftCardUsageID] ASC
        )
    )
    GO




    if not exists (select * From sysobjects where id = object_id('ProductDistributor') and type = 'u') begin
        CREATE TABLE [dbo].[ProductDistributor](
            [ProductID] [int] NOT NULL,
            [DistributorID] [int] NOT NULL,
            [DisplayOrder] [int] NOT NULL CONSTRAINT DF_ProductDistributor_DisplayOrder DEFAULT((1)),
            [CreatedOn] [datetime] NOT NULL CONSTRAINT DF_ProductDistributor_CreatedOn DEFAULT(getdate()),
            CONSTRAINT [PK_ProductDistributor] PRIMARY KEY CLUSTERED 
            (
                [ProductID] ASC,
                [DistributorID] ASC
            )
        )

    end
    GO



    if not exists (select * From sysobjects where id = object_id('ProductManufacturer') and type = 'u') begin
        CREATE TABLE [dbo].[ProductManufacturer](
            [ProductID] [int] NOT NULL,
            [ManufacturerID] [int] NOT NULL,
            [DisplayOrder] [int] NOT NULL CONSTRAINT DF_ProductManufacturer_DisplayOrder DEFAULT(1),
            [CreatedOn] [datetime] NOT NULL CONSTRAINT DF_ProductManufacturer_CreatedOn DEFAULT(getdate()),
            CONSTRAINT [PK_ProductManufacturer] PRIMARY KEY CLUSTERED 
            (
                [ProductID] ASC,
                [ManufacturerID] ASC
            )
        )
    end
    GO


    if not exists (select * From sysobjects where id = object_id('ErrorLog') and type = 'u') begin
        CREATE TABLE [dbo].[ErrorLog](
            [logid] [int] IDENTITY(1,1) NOT NULL,
            [errorDt] [datetime] NOT NULL CONSTRAINT DF_ErrorLog_errorDt DEFAULT(getdate()),
            [source] [varchar](400) NULL,
            [errormsg] [text] NULL,
            CONSTRAINT [PK_ErrorLog] PRIMARY KEY CLUSTERED 
            (
                [logid] ASC
            )
        )
    end
    GO







    if not exists (select * From sysobjects where id = object_id('Currency') and type = 'u') begin
        CREATE TABLE [dbo].[Currency](
            [CurrencyID] [int] IDENTITY(1,1) NOT NULL,
            [CurrencyGUID] [uniqueidentifier] NOT NULL CONSTRAINT DF_Currency_CurrencyGUID DEFAULT(newid()),
            [Name] [nvarchar](100) NOT NULL,
            [CurrencyCode] [nvarchar](10) NOT NULL,
            [ExchangeRate] [money] NULL,
            [WasLiveRate] [tinyint] NOT NULL CONSTRAINT DF_Currency_WasLiveRate DEFAULT((0)),
            [DisplayLocaleFormat] [nvarchar](10) NULL,
            [Symbol] [nvarchar](10) NULL,
            [ExtensionData] [ntext] NULL,
            [Published] [tinyint] NOT NULL CONSTRAINT DF_Currency_Published DEFAULT((1)),
            [DisplayOrder] [int] NOT NULL CONSTRAINT DF_Currency_DisplayOrder DEFAULT((1)),
            [DisplaySpec] [nvarchar](20) NULL,
            [CreatedOn] [datetime] NOT NULL CONSTRAINT DF_Currency_CreatedOn DEFAULT(getdate()),
            [LastUpdated] [datetime] NOT NULL CONSTRAINT DF_Currency_LastUpdated DEFAULT (getdate()),
            CONSTRAINT [PK_Currency] PRIMARY KEY CLUSTERED 
            (
                [CurrencyID] ASC
            )
        )
    end
    GO






    if not exists (select * From sysobjects where id = object_id('ShippingByProduct') and type = 'u') begin
        CREATE TABLE [dbo].[ShippingByProduct](
            [ShippingByProductID] [int] IDENTITY(1,1) NOT NULL,
            [ShippingByProductGUID] [uniqueidentifier] NOT NULL CONSTRAINT DF_ShippingByProduct_ShippingByProductGUID DEFAULT(newid()),
            [VariantID] [int] NOT NULL,
            [ShippingMethodID] [int] NOT NULL,
            [ShippingCost] [money] NOT NULL,
            [CreatedOn] [datetime] NOT NULL CONSTRAINT DF_ShippingByProduct_CreatedOn DEFAULT(getdate()),
            CONSTRAINT [PK_ShippingByProduct] PRIMARY KEY CLUSTERED 
            (
                [ShippingByProductID] ASC
            )
            
        )
    end
    GO












    if not exists (select * From syscolumns where id = object_id('ProductVariant') and name = 'SEAltText') begin
        alter table [dbo].ProductVariant ADD SEAltText [ntext] null
    end
    go    
    
    if not exists (select * From syscolumns where id = object_id('ShoppingCart') and name = 'IsKit2') begin
		alter table [dbo].[ShoppingCart] ADD [IsKit2] tinyint NOT NULL CONSTRAINT DF_ShoppingCartIsKit2 DEFAULT(0)
	end
	go        


    if not exists(select * from syscolumns where id = object_id('Appconfig') and name = 'Hidden') begin
        alter table [dbo].[Appconfig] ADD [Hidden] bit NOT NULL CONSTRAINT DF_AppConfig_Hidden default(0)
    end
    go



    if not exists (select * From syscolumns where id = object_id('PasswordLog') and name = 'SaltKey') begin
        alter table [dbo].[PasswordLog] ADD [SaltKey] int NOT NULL CONSTRAINT DF_PasswordLogSaltKey DEFAULT(0)
    end
    go



    if not exists (select * From syscolumns where id = object_id('FailedTransaction') and name = 'MaxMindFraudScore') begin
        alter table dbo.FailedTransaction add [MaxMindFraudScore] decimal(5,2) NULL CONSTRAINT DF_FailedTransaction_MaxMindScore DEFAULT(-1)
    end
    go

    IF not exists (select * From syscolumns where id = object_id('FailedTransaction') and name = 'MaxMindDetails')  
        alter table  [dbo].[FailedTransaction] add  [MaxMindDetails] [ntext]
    go

    if not exists (select * From syscolumns where id = object_id('FailedTransaction') and name = 'RecurringSubscriptionID') begin
        alter table dbo.FailedTransaction add [RecurringSubscriptionID] [nvarchar](100) NOT NULL CONSTRAINT DF_FailedTransaction_RecurringSubscriptionID DEFAULT('')
    end
    go

    if not exists (select * From syscolumns where id = object_id('FailedTransaction') and name = 'CustomerEMailed') begin
        alter table dbo.FailedTransaction add [CustomerEMailed] [tinyint] NOT NULL CONSTRAINT DF_FailedTransaction_CustomerEMailed DEFAULT(0) 
    end
    go

    if not exists (select * From syscolumns where id = object_id('FailedTransaction') and name = 'IPAddress')
        alter table dbo.FailedTransaction add [IPAddress] [nvarchar] (25) NULL
    go

    if not exists (select * From syscolumns where id = object_id('FailedTransaction') and name = 'OrderNumber')
        alter table dbo.FailedTransaction add [OrderNumber] [int] NOT NULL CONSTRAINT DF_FailedTransaction_OrderNumber DEFAULT((0))
    go

    if not exists (select * From syscolumns where id = object_id('FailedTransaction') and name = 'PaymentMethod')
        alter table dbo.FailedTransaction add [PaymentMethod] [nvarchar](50) NULL 
    go








    if not exists (select * From syscolumns where id = object_id('CountryTaxRate') and name = 'CountryTaxID')  
        alter table dbo.CountryTaxRate add [CountryTaxID] int NOT NULL IDENTITY;
    go

    if exists (select * From syscolumns where id = object_id('State') and name = 'TaxRate') begin
        DECLARE @cmd varchar(8000) 
        SET @cmd = 'INSERT dbo.StateTaxRate (StateID, TaxRate)  SELECT StateID, isnull(TaxRate, 0) FROM dbo.State'
        EXEC (@cmd) 
        SET @cmd = 'alter table dbo.State drop column TaxRate'
        EXEC (@cmd) 
    end
    go

    if not exists (select * From syscolumns where id = object_id('StateTaxRate') and name = 'StateTaxID')  
        alter table dbo.StateTaxRate add [StateTaxID] int NOT NULL IDENTITY;
    go




    if exists (select * From syscolumns where id = object_id('Country') and name = 'TaxRate')  begin
        DECLARE @cmd varchar(8000) 
        SET @cmd = 'INSERT dbo.CountryTaxRate (CountryID, TaxRate) SELECT CountryID, isnull(TaxRate, 0) FROM dbo.Country'
        EXEC (@cmd) 

        exec aspdnsf_DropColumnWithDefaultConstraint 'Country','TaxRate';

    end

    go



    if not exists (select * From syscolumns where id = object_id('ZipTaxRate') and name = 'TaxClassID') 
        alter table dbo.ZipTaxRate add [TaxClassID] [int] NOT NULL CONSTRAINT DF_ZipTaxRate_TaxClassID DEFAULT((1))
    go

    IF not exists (select * From syscolumns where id = object_id('ZipTaxRate') and name = 'ZipTaxID') BEGIN
        if exists (select * from sysobjects where name = 'DF_ZipTaxRate_CreatedOn' and type = 'D')
            ALTER TABLE dbo.ZipTaxRate DROP CONSTRAINT DF_ZipTaxRate_CreatedOn
        ALTER TABLE dbo.ZipTaxRate DROP CONSTRAINT DF_ZipTaxRate_TaxClassID
        ALTER TABLE dbo.ZipTaxRate DROP CONSTRAINT PK_ZipTaxRate


        CREATE TABLE [dbo].[ZipTaxRate_new](
            [ZipTaxID] int NOT NULL IDENTITY,
            [ZipCode] [nvarchar](10) NOT NULL,
            [TaxClassID] [int] NOT NULL CONSTRAINT DF_ZipTaxRate_TaxClassID DEFAULT((1)),
            [TaxRate] [money] NULL,
            [CreatedOn] [datetime] NOT NULL CONSTRAINT DF_ZipTaxRate_CreatedOn DEFAULT(getdate()),
            CONSTRAINT [PK_ZipTaxRate] PRIMARY KEY CLUSTERED 
            (
                [ZipTaxID] ASC
            )
        )

        INSERT dbo.ZipTaxRate_new (ZipCode, TaxClassID, TaxRate, CreatedOn) 
        SELECT ZipCode, TaxClassID, TaxRate, CreatedOn FROM dbo.ZipTaxRate

        DROP TABLE dbo.ZipTaxRate

        exec sp_rename 'ZipTaxRate_new', 'ZipTaxRate'
        CREATE UNIQUE INDEX [UIX_ZipTaxRate_ZipCode_TaxClassID] ON [ZipTaxRate] (ZipCode,TaxClassID)
    END
    GO







    DECLARE @dropcmd varchar(8000)
    SET @dropcmd = ''
    select @dropcmd = @dropcmd + ' ALTER TABLE ' + so.name + ' DROP CONSTRAINT ' + so2.name 
    From sysobjects so join syscolumns sc on so.id = sc.id join systypes st on sc.xtype = st.xtype join sysobjects so2 on sc.cdefault = so2.id
    where so.xtype = 'u' and st.name in ('float' , 'real')
    order by so.name, sc.name

    EXEC (@dropcmd)

    GO


    if exists (select * from sysindexes where name = 'IX_ShippingMethod_DisplayOrder_Name')
        DROP INDEX ShippingMethod.IX_ShippingMethod_DisplayOrder_Name

    if exists (select * from sysindexes where name = 'IX_ShippingByTotal_ShippingMethodID_LowValue_HighValue')
        DROP INDEX ShippingByTotal.IX_ShippingByTotal_ShippingMethodID_LowValue_HighValue
    GO
    if exists (select * from sysindexes where name = 'IX_ShippingByWeight_ShippingMethodID_LowValue_HighValue')
        DROP INDEX ShippingByWeight.IX_ShippingByWeight_ShippingMethodID_LowValue_HighValue
    GO
    if exists (select * from sysindexes where name = 'IX_ShippingWeightByZone_ShippingZoneID_LowValue_HighValue')
        DROP INDEX ShippingWeightByZone.IX_ShippingWeightByZone_ShippingZoneID_LowValue_HighValue
    GO



    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('Coupon') and sc.name = 'DiscountPercent'  and st.name in ('real', 'float') )
        ALTER TABLE Coupon ALTER COLUMN DiscountPercent money not null ALTER TABLE Coupon ADD  CONSTRAINT DF_Coupon_DiscountPercent DEFAULT ((0.0)) FOR DiscountPercent
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('CustomCart') and sc.name = 'ProductWeight'  and st.name in ('real', 'float') )
        ALTER TABLE CustomCart ALTER COLUMN ProductWeight money not null ALTER TABLE CustomCart ADD  CONSTRAINT DF_CustomCart_ProductWeight DEFAULT ((0.0)) FOR ProductWeight
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('CustomerLevel') and sc.name = 'LevelDiscountPercent'  and st.name in ('real', 'float') )
        ALTER TABLE CustomerLevel ALTER COLUMN LevelDiscountPercent money not null ALTER TABLE CustomerLevel ADD  CONSTRAINT DF_CustomerLevel_LevelDiscountPercent DEFAULT ((0.0)) FOR LevelDiscountPercent
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('FraudOrders') and sc.name = 'CountryTaxRate'  and st.name in ('real', 'float') )
        ALTER TABLE FraudOrders ALTER COLUMN CountryTaxRate money null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('FraudOrders') and sc.name = 'CouponDiscountPercent'  and st.name in ('real', 'float') )
        ALTER TABLE FraudOrders ALTER COLUMN CouponDiscountPercent money null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('FraudOrders') and sc.name = 'LevelDiscountPercent'  and st.name in ('real', 'float') )
        ALTER TABLE FraudOrders ALTER COLUMN LevelDiscountPercent money null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('FraudOrders') and sc.name = 'OrderWeight'  and st.name in ('real', 'float') )
        ALTER TABLE FraudOrders ALTER COLUMN OrderWeight money null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('FraudOrders') and sc.name = 'StateTaxRate'  and st.name in ('real', 'float') )
        ALTER TABLE FraudOrders ALTER COLUMN StateTaxRate money null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('FraudOrders_CustomCart') and sc.name = 'ProductWeight'  and st.name in ('real', 'float') )
        ALTER TABLE FraudOrders_CustomCart ALTER COLUMN ProductWeight money null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('FraudOrders_ShoppingCart') and sc.name = 'OrderedProductQuantityDiscountPercent'  and st.name in ('real', 'float') )
        ALTER TABLE FraudOrders_ShoppingCart ALTER COLUMN OrderedProductQuantityDiscountPercent money null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('Orders') and sc.name = 'CountryTaxRate'  and st.name in ('real', 'float') )
        ALTER TABLE Orders ALTER COLUMN CountryTaxRate money null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('Orders') and sc.name = 'CouponDiscountPercent'  and st.name in ('real', 'float') )
        ALTER TABLE Orders ALTER COLUMN CouponDiscountPercent money null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('Orders') and sc.name = 'LevelDiscountPercent'  and st.name in ('real', 'float') )
        ALTER TABLE Orders ALTER COLUMN LevelDiscountPercent money null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('Orders') and sc.name = 'OrderWeight'  and st.name in ('real', 'float') )
        ALTER TABLE Orders ALTER COLUMN OrderWeight money not null ALTER TABLE Orders ADD  CONSTRAINT DF_Orders_OrderWeight DEFAULT ((0.0)) FOR OrderWeight
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('Orders') and sc.name = 'StateTaxRate'  and st.name in ('real', 'float') )
        ALTER TABLE Orders ALTER COLUMN StateTaxRate money null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('Orders_CustomCart') and sc.name = 'ProductWeight'  and st.name in ('real', 'float') )
        ALTER TABLE Orders_CustomCart ALTER COLUMN ProductWeight money null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('Orders_ShoppingCart') and sc.name = 'OrderedProductQuantityDiscountPercent'  and st.name in ('real', 'float') )
        ALTER TABLE Orders_ShoppingCart ALTER COLUMN OrderedProductQuantityDiscountPercent money null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('Product') and sc.name = 'UpsellProductDiscountPercentage'  and st.name in ('real', 'float') )
        ALTER TABLE Product ALTER COLUMN UpsellProductDiscountPercentage money not null ALTER TABLE Product ADD  CONSTRAINT DF_Product_UpsellProductDiscountPercentage DEFAULT ((0.0)) FOR UpsellProductDiscountPercentage
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('ProductVariant') and sc.name = 'Weight'  and st.name in ('real', 'float') )
        ALTER TABLE ProductVariant ALTER COLUMN Weight money null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('QuantityDiscountTable') and sc.name = 'DiscountPercent'  and st.name in ('real', 'float') )
        ALTER TABLE QuantityDiscountTable ALTER COLUMN DiscountPercent money not null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('ShippingByTotal') and sc.name = 'HighValue'  and st.name in ('real', 'float') )
        ALTER TABLE ShippingByTotal ALTER COLUMN HighValue money not null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('ShippingByTotal') and sc.name = 'LowValue'  and st.name in ('real', 'float') )
        ALTER TABLE ShippingByTotal ALTER COLUMN LowValue money not null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('ShippingByWeight') and sc.name = 'HighValue'  and st.name in ('real', 'float') )
        ALTER TABLE ShippingByWeight ALTER COLUMN HighValue money not null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('ShippingByWeight') and sc.name = 'LowValue'  and st.name in ('real', 'float') )
        ALTER TABLE ShippingByWeight ALTER COLUMN LowValue money not null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('ShippingMethod') and sc.name = 'FixedPercentOfTotal'  and st.name in ('real', 'float') )
        ALTER TABLE ShippingMethod ALTER COLUMN FixedPercentOfTotal money null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('ShippingMethod') and sc.name = 'FixedRate'  and st.name in ('real', 'float') )
        ALTER TABLE ShippingMethod ALTER COLUMN FixedRate money null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('ShippingWeightByZone') and sc.name = 'HighValue'  and st.name in ('real', 'float') )
        ALTER TABLE ShippingWeightByZone ALTER COLUMN HighValue money not null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('ShippingWeightByZone') and sc.name = 'LowValue'  and st.name in ('real', 'float') )
        ALTER TABLE ShippingWeightByZone ALTER COLUMN LowValue money not null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('ShoppingCart') and sc.name = 'ProductWeight'  and st.name in ('real', 'float') )
        ALTER TABLE ShoppingCart ALTER COLUMN ProductWeight money null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('State') and sc.name = 'TaxRate'  and st.name in ('real', 'float') )
        ALTER TABLE State ALTER COLUMN TaxRate money null
    go
    if exists (select * from syscolumns sc join sysobjects so on sc.id = so.id join systypes st on sc.xtype = st.xtype where so.id = object_id('ZipTaxRate') and sc.name = 'TaxRate'  and st.name in ('real', 'float') )
        ALTER TABLE ZipTaxRate ALTER COLUMN TaxRate money null
    go


    -- Tracking Requires Added counts


    if not exists (select * From syscolumns with (nolock) where id = object_id('ProductCategory') and name = 'DisplayOrder')
        ALTER TABLE dbo.ProductCategory ADD DisplayOrder int;
    go

    if not exists (select * From syscolumns with (nolock) where id = object_id('ProductSection') and name = 'DisplayOrder')
        ALTER TABLE dbo.ProductSection ADD DisplayOrder int ;
    go

    if not exists (select * From syscolumns with (nolock) where id = object_id('DocumentLibrary') and name = 'DisplayOrder')
        ALTER TABLE dbo.DocumentLibrary ADD DisplayOrder int;
    go



    -- transform data from  old display order tables
    UPDATE [dbo].ProductCategory 
    SET DisplayOrder = isnull(cdo.DisplayOrder, 1)
    FROM ProductCategory pc
        left join [dbo].CategoryDisplayOrder cdo on pc.ProductID = cdo.ProductID and pc.CategoryID = cdo.CategoryID
    WHERE pc.DisplayOrder is null
    GO








    UPDATE [dbo].ProductSection 
    SET DisplayOrder = isnull(sdo.DisplayOrder, 1)
    FROM ProductSection ps
        left join [dbo].SectionDisplayOrder sdo on ps.ProductID = sdo.ProductID and ps.SectionID = sdo.SectionID;
    GO



    INSERT [dbo].ProductDistributor (ProductID, DistributorID, DisplayOrder)
    SELECT p.ProductID, p.DistributorID, 1
    FROM Product p 
    WHERE DistributorID > 0;
    GO




    INSERT [dbo].ProductManufacturer (ProductID, ManufacturerID, DisplayOrder)
    SELECT p.ProductID, p.ManufacturerID, 1
    FROM Product p ;
    GO



    UPDATE [dbo].DocumentLibrary 
    SET DisplayOrder = isnull(ldo.DisplayOrder, 1)
    FROM DocumentLibrary  dl
        left join [dbo].LibraryDisplayOrder ldo on dl.LibraryID = ldo.LibraryID and dl.DocumentID = ldo.DocumentID;
    GO
    -- end transformation





    if exists (select * from [dbo].syscolumns with (nolock) where id = object_id('ProductCategory') and name = 'DisplayOrder' and isnullable = 1)
        ALTER TABLE dbo.ProductCategory ALTER COLUMN DisplayOrder int NOT NULL;
    GO

    if exists (select * from [dbo].syscolumns with (nolock) where id = object_id('ProductCategory') and name = 'DisplayOrder' and cdefault = 0)
        ALTER TABLE dbo.ProductCategory ADD CONSTRAINT DF_ProductCategory_DisplayOrder DEFAULT 1 FOR DisplayOrder;
    GO





    if exists (select * from [dbo].syscolumns with (nolock) where id = object_id('ProductSection') and name = 'DisplayOrder' and isnullable = 1)
        ALTER TABLE dbo.ProductSection ALTER COLUMN DisplayOrder int NOT NULL;
    GO

    if exists (select * from [dbo].syscolumns with (nolock) where id = object_id('ProductSection') and name = 'DisplayOrder' and cdefault = 0)
        ALTER TABLE dbo.ProductSection ADD CONSTRAINT DF_ProductSection_DisplayOrder DEFAULT 1 FOR DisplayOrder;
    GO





    if exists (select * from [dbo].syscolumns with (nolock) where id = object_id('DocumentLibrary') and name = 'DisplayOrder' and isnullable = 1)
        ALTER TABLE dbo.DocumentLibrary ALTER COLUMN DisplayOrder int NOT NULL;
    GO

    if exists (select * from [dbo].syscolumns with (nolock) where id = object_id('DocumentLibrary') and name = 'DisplayOrder' and cdefault = 0)
        ALTER TABLE dbo.DocumentLibrary ADD CONSTRAINT DF_DocumentLibrary_DisplayOrder DEFAULT 1 FOR DisplayOrder;
    GO







    declare @cmd varchar(8000), @defaultname varchar(800)
    if exists (select * From syscolumns where id = object_id('GiftCard') and name = 'IsEmailGiftCard') begin
        select @defaultname = name From sysobjects where id = (select cdefault From syscolumns where id = object_id('GiftCard') and name = 'IsEmailGiftCard')
        
        if @defaultname is not null begin
            select @cmd = 'alter table dbo.GiftCard drop constraint ' + @defaultname
            EXEC (@cmd)
        end
        alter table dbo.GiftCard drop column IsEmailGiftCard
    end
    go

    if not exists (select * From syscolumns where id = object_id('GiftCard') and name = 'GiftCardTypeID') 
        alter table  dbo.GiftCard add  GiftCardTypeID int NULL CONSTRAINT DF_GiftCard_GiftCardTypeID DEFAULT((101))
    go

    if not exists (select * From syscolumns where id = object_id('GiftCard') and name = 'DisabledByAdministrator') 
        alter table dbo.GiftCard add  DisabledByAdministrator tinyint default(0) NOT NULL
    go




    if exists (select * From syscolumns where id = object_id('GiftCardUsage') and name = 'SerialNumber') begin
        if exists (select * From sysindexes where name = 'IX_GiftCardUsage_SerialNumber')
            drop index GiftCardUsage.IX_GiftCardUsage_SerialNumber
        alter table dbo.GiftCardUsage drop column SerialNumber
    end
    go

    if not exists (select * From syscolumns where id = object_id('GiftCardUsage') and name = 'UsageTypeID') 
        alter table dbo.GiftCardUsage add UsageTypeID int  NOT NULL default(0)
    go











    if not exists (select * From syscolumns with (nolock) where id = object_id('Product') and name = 'WarehouseLocation')
        ALTER TABLE dbo.Product ADD WarehouseLocation nvarchar(100) NULL;
    go


    IF exists (select * from dbo.sysindexes where name = 'IX_Product_ManufacturerID')
        DROP INDEX Product.IX_Product_ManufacturerID
    GO

    IF exists (select * from dbo.sysindexes where name = 'IX_Product_DistributorID')
        DROP INDEX Product.IX_Product_DistributorID
    GO

    declare @def varchar(255)
    select @def = so.name From dbo.sysobjects so join [dbo].syscolumns sc on so.id = sc.cdefault and sc.id = object_id('product') and sc.name = 'ManufacturerID'

    exec ('ALTER TABLE dbo.Product DROP CONSTRAINT ' + @def)

    select @def = so.name From sysobjects so join [dbo].syscolumns sc on so.id = sc.cdefault and sc.id = object_id('product') and sc.name = 'DistributorID'
    exec ('ALTER TABLE dbo.Product DROP CONSTRAINT ' + @def)
    GO

    if exists (select * From syscolumns with (nolock) where id = object_id('Product') and name = 'ManufacturerID')
        ALTER  TABLE dbo.Product DROP COLUMN ManufacturerID, DistributorID;
    go

    IF not exists (select * From dbo.syscolumns where id = object_id('Product') and name = 'PageSize')
        alter table dbo.Product add [PageSize] [int] default(20) NOT NULL;
    GO

    if not exists (select * From syscolumns with (nolock) where id = object_id('Product') and name = 'Wholesale')
        alter table dbo.Product add [Wholesale] [tinyint] CONSTRAINT DF_Product_Wholesale default(0) NOT NULL;
    go

    if not exists (select * from syscolumns where id = object_id('Product')  and name = 'SkinID' )
        alter table dbo.Product 
        add [SkinID] int NOT NULL CONSTRAINT DF_Product_SkinID DEFAULT(0),
            [TemplateName] nvarchar(50) NOT NULL CONSTRAINT DF_Product_TemplateName DEFAULT('')
    go

    if not exists (select * From syscolumns where id = object_id('Product') and name = 'GoogleCheckoutAllowed')
     alter table dbo.Product add [GoogleCheckoutAllowed] [tinyint] NOT NULL CONSTRAINT DF_Product_GoogleCheckoutAllowed DEFAULT((1));
    go

    if not exists (select * From syscolumns where id = object_id('Product') and name = 'AvailableStartDate')
        alter table dbo.Product add [AvailableStartDate] [datetime] NOT NULL CONSTRAINT DF_Product_AvailableStartDate DEFAULT(getdate()), -- field for future use. not used now
                                    [AvailableStopDate] [datetime] NULL -- if null, there is no end date
    go

    if not exists (select * From syscolumns where id = object_id('Product') and name = 'TaxClassID')  
        alter table Product add [TaxClassID] [int] NOT NULL CONSTRAINT DF_Product_TaxClassID DEFAULT((1))
    go

    if not exists (select * From syscolumns with (nolock) where id = object_id('Product') and name = 'WarehouseLocation')
        ALTER TABLE dbo.Product ADD WarehouseLocation nvarchar(100) NULL;
    go
















    if not exists (select * From syscolumns where id = object_id('KitItem') and name = 'InventoryVariantID') begin
        alter table dbo.KitItem add [InventoryVariantID]    [int] NULL,
                                    [InventoryVariantColor] [nvarchar] (100) NULL, -- must be in master store locale language, this is NOT a ml field!
                                    [InventoryVariantSize]  [nvarchar] (100) NULL -- must be in master store locale language, this is NOT a ml field!
    end
    go

    if not exists (select * From syscolumns where id = object_id('KitItem') and name = 'WeightDelta') begin
        alter table dbo.KitItem add [WeightDelta] [money] NOT NULL CONSTRAINT DF_KitItem_WeightDelta DEFAULT(0.0)
    end
    go












    if not exists (select * From syscolumns where id = object_id('Orders_KitCart') and name = 'InventoryVariantID') begin
        alter table dbo.Orders_KitCart add [InventoryVariantID]    [int] NULL,
                                           [InventoryVariantColor] [nvarchar] (100) NULL, -- must be in master store locale language, this is NOT a ml field!
                                           [InventoryVariantSize]  [nvarchar] (100) NULL -- must be in master store locale language, this is NOT a ml field!
    end
    go

    if not exists (select * From syscolumns where id = object_id('Orders_KitCart') and name = 'KitItemWeightDelta') 
          alter table dbo.Orders_KitCart add [KitItemWeightDelta] money NOT NULL CONSTRAINT DF_Orders_KitCart_WeightDelta DEFAULT (0.0)
    go


    alter table dbo.Orders_KitCart alter column [KitGroupName] [nvarchar](400) NULL
    go

    if not exists (select * From syscolumns where id = object_id('Orders_KitCart') and name = 'KitGroupTypeID')  begin
        alter table dbo.Orders_KitCart add [KitGroupTypeID] [int] NULL 
    end
    go

    update dbo.orders_kitcart set KitGroupTypeID = isnull(kg.KitGroupTypeID, 0) from dbo.orders_kitcart kc left join dbo.KitGroup kg on kc.kitgroupid = kg.kitgroupid where kc.KitGroupTypeID is null
    alter table dbo.Orders_KitCart alter column [KitGroupTypeID] [int] NOT NULL 
    go


    if exists (select * From syscolumns where id = object_id('Orders_KitCart') and name = 'KitItemName') begin
        ALTER TABLE dbo.Orders_KitCart ALTER COLUMN KitItemName [nvarchar](400) NULL
    end
    go












    if not exists (select * From syscolumns where id = object_id('KitCart') and name = 'InventoryVariantID') begin
        alter table dbo.KitCart add [InventoryVariantID]    [int] NULL,
                                    [InventoryVariantColor] [nvarchar] (100) NULL, -- must be in master store locale language, this is NOT a ml field!
                                    [InventoryVariantSize]  [nvarchar] (100) NULL -- must be in master store locale language, this is NOT a ml field!
    end
    go

    if not exists (select * From syscolumns where id = object_id('KitCart') and name = 'KitGroupTypeID') begin
        alter table dbo.KitCart add [KitGroupTypeID] [int] NULL 
    end
    go

    update  dbo.KitCart set KitGroupTypeID = isnull(kg.KitGroupTypeID, 0) from dbo.KitCart kc left join dbo.KitGroup kg on kc.kitgroupid = kg.kitgroupid where kc.KitGroupTypeID is null
    alter table dbo.KitCart alter column [KitGroupTypeID] [int] NOT NULL 
    go














    if not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('customer') and name = 'CurrencySetting')
        alter table dbo.customer add [CurrencySetting] [nvarchar] (10) NULL;
    go

    if not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('customer') and name = 'Over13Checked')
        alter table dbo.Customer add [Over13Checked] [tinyint] NULL;
    go

    if exists (select * from [dbo].syscolumns with (nolock) where id = object_id('customer') and name = 'RecurringShippingMethod')
        ALTER TABLE [dbo].[customer] ALTER COLUMN RecurringShippingMethod [ntext] NULL
    go

    if not exists (select * From syscolumns where id = object_id('Customer') and name = 'RequestedPaymentMethod') begin
        alter table [dbo].[Customer] ADD [RequestedPaymentMethod] nvarchar(100) null
    end
    go

    alter table  [dbo].[Customer] ALTER COLUMN Password [nvarchar] (250)
    go


    if exists (select * from syscolumns where id = object_id('Customer')  and name = 'Password' and cdefault = 0)
        alter table dbo.customer add constraint DF_Customer_Password DEFAULT '' FOR Password
    go


    if not exists (select * from syscolumns where id = object_id('Customer')  and name = 'FAX' )
        alter table dbo.customer add FAX nvarchar(25) null
    go

    if exists (select * from syscolumns where id = object_id('Customer')  and name = 'LastIPAddress' )
        alter table dbo.customer alter column LastIPAddress varchar(40) null
    go

    if exists (select * From syscolumns where id = object_id('Customer') and name = 'PaymentMethod')
        alter table dbo.Customer drop column PaymentMethod;
    go


    if exists (select * From syscolumns where id = object_id('Customer') and name = 'ButterfliesShown')
        exec aspdnsf_DropColumnWithDefaultConstraint 'Customer','ButterfliesShown';
    go

    if not exists (select * From syscolumns where id = object_id('Customer') and name = 'VATSetting') begin
        alter table dbo.Customer add [VATSetting] int NOT NULL CONSTRAINT DF_customer_VATSetting default(2), -- 1 = show prices inclusive of VAT, 2 = show prices exclusive of VAT
        [VATRegistrationID] [nvarchar](100) NOT NULL CONSTRAINT DF_customer_VATRegistrationID default('')
    end
    go

    if not exists (select * From syscolumns where id = object_id('customer') and name = 'StoreCCInDB') 
        alter table dbo.customer add StoreCCInDB tinyint not null constraint DF_customer_StoreCCInDB default(1)
    go

    if not exists (select * From syscolumns where id = object_id('customer') and name = 'IsRegistered') 
        alter table dbo.customer add IsRegistered tinyint not null constraint DF_customer_IsRegistered default(0)
    go

    if not exists (select * From syscolumns where id = object_id('customer') and name = 'LockedUntil') 
        alter table dbo.customer add LockedUntil datetime
    go

    if not exists (select * From syscolumns where id = object_id('customer') and name = 'AdminCanViewCC') 
        alter table dbo.customer add AdminCanViewCC tinyint not null constraint DF_customer_AdminCanViewCC default(1)
    go

    if not exists (select * From syscolumns where id = object_id('customer') and name = 'PwdChanged') 
        alter table dbo.customer add PwdChanged datetime not null constraint DF_customer_PwdChanged default(getdate())
    go

    if not exists (select * From syscolumns where id = object_id('customer') and name = 'BadLoginCount') 
        alter table dbo.customer add BadLoginCount tinyint not null constraint DF_customer_BadLoginCount default(0)
    go

    if not exists (select * From syscolumns where id = object_id('customer') and name = 'LastBadLogin') 
        alter table dbo.customer add LastBadLogin datetime 
    go

    if not exists (select * From syscolumns where id = object_id('customer') and name = 'Active') 
        alter table dbo.customer add Active tinyint not null constraint DF_customer_Active default(1)
    go

    if not exists (select * From syscolumns where id = object_id('customer') and name = 'PwdChangeRequired')
        alter table dbo.customer add PwdChangeRequired tinyint null
    go

    if not exists (select * From syscolumns where id = object_id('customer') and name = 'SaltKey') begin
        alter table dbo.customer add SaltKey int
        declare @cmd varchar(8000) 
        if (select isnull(configvalue, '') from dbo.appconfig where name = 'EncryptPassword') = 'true'
            set @cmd = 'update dbo.customer set SaltKey = -2'
        else
            set @cmd = 'update dbo.customer set SaltKey = -1'
        exec (@cmd)

        alter table dbo.customer alter column [SaltKey] [int] NOT NULL 

        alter table dbo.customer add constraint DF_Customer_SaltKey DEFAULT 0 FOR SaltKey
    end
    go

    if not exists (select * From syscolumns where id = object_id('Customer') and name = 'SkinID')
        exec sp_rename 'dbo.Customer.SiteID', 'SkinID', 'COLUMN'    
    go

    if not exists (select * From syscolumns where id = object_id('Customer') and name = 'BuySafe') 
        alter table [dbo].[Customer] ADD BuySafe money null
    go















    if not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('OrderOption') and name = 'DefaultIsChecked')
        alter table dbo.OrderOption add [DefaultIsChecked] [tinyint] default(0) NOT NULL;
    go

    if not exists (select * From syscolumns where id = object_id('OrderOption') and name = 'TaxClassID')
       alter table dbo.OrderOption add [TaxClassID] [int] NOT NULL CONSTRAINT DF_OrderOption_TaxClassID DEFAULT((1))
    go









    IF not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('Topic') and name = 'HTMLOk')
        alter table [dbo].Topic add [HTMLOk] [tinyint] default(1) NOT NULL;
    GO

    if not exists (select * From syscolumns with (nolock) where id = object_id('Topic') and name = 'SENoScript')
        alter table dbo.Topic add [SENoScript] [ntext]; -- locale Xml field
    go

    if not exists (select * From syscolumns where id = object_id('Topic') and name = 'SkinID')
        exec sp_rename 'dbo.Topic.SiteID', 'SkinID', 'COLUMN'
    go

    if not exists (select * from syscolumns where id = object_id('Topic')  and name = 'DisplayOrder' )
        ALTER TABLE [dbo].[Topic] ADD [DisplayOrder] [int] NOT NULL CONSTRAINT [DF_Topic_DisplayOrder]  DEFAULT ((1))
    go









    if not exists (select * From syscolumns where id = object_id('Country') and name = 'Published')  
       alter table dbo.Country add [Published] [tinyint] NOT NULL CONSTRAINT DF_Country_Published DEFAULT((1))
    go












    if not exists (select * From syscolumns where id = object_id('State') and name = 'Published')  
       alter table dbo.State add [Published] [tinyint] NOT NULL CONSTRAINT DF_State_Published DEFAULT((1))
    go						 
























    IF exists (select * from [dbo].syscolumns with (nolock) where id = object_id('FraudOrders') and name = 'ShippingMethod')
        alter table [dbo].FraudOrders alter column [ShippingMethod] [ntext];
    go

    if not exists (select * From syscolumns where id = object_id('FraudOrders') and name = 'InventoryWasReduced')
        alter table dbo.FraudOrders add InventoryWasReduced int NOT NULL default(0)
    go

    if exists (select * From syscolumns where id = object_id('FraudOrders') and name = 'SubscriptionIntervalType') begin
        declare @cmd varchar(8000), @defaultname varchar(800)
        select @defaultname = name From sysobjects where id = (select cdefault From syscolumns where id = object_id('FraudOrders') and name = 'SubscriptionIntervalType')
        
        if @defaultname is not null begin
            select @cmd = 'alter table dbo.FraudOrders drop constraint ' + @defaultname
            EXEC (@cmd)
        end
        alter table dbo.FraudOrders drop column SubscriptionIntervalType 
        
    end
    go

    if not exists (select * From syscolumns where id = object_id('FraudOrders') and name = 'TrackingURL') 
        alter table  dbo.FraudOrders add  TrackingURL ntext NULL
    go

    if not exists (select * From syscolumns where id = object_id('FraudOrders') and name = 'THUB_POSTED_TO_ACCOUNTING') 
        alter table  dbo.FraudOrders add  THUB_POSTED_TO_ACCOUNTING char(1) NULL 
    go

    if not exists (select * From syscolumns where id = object_id('FraudOrders') and name = 'THUB_POSTED_DATE') 
        alter table  dbo.FraudOrders add  THUB_POSTED_DATE datetime NULL
    go

    if not exists (select * From syscolumns where id = object_id('FraudOrders') and name = 'THUB_ACCOUNTING_REF') 
        alter table  dbo.FraudOrders add  THUB_ACCOUNTING_REF char(25) NULL
    go

    if not exists (select * From syscolumns where id = object_id('FraudOrders') and name = 'Last4') 
        alter table  dbo.FraudOrders add  Last4 nvarchar(4) NULL
    go

    if not exists (select * From syscolumns where id = object_id('FraudOrders') and name = 'ReadyToShip') 
        alter table  dbo.FraudOrders add   ReadyToShip tinyint NULL 
    go

    if not exists (select * From syscolumns where id = object_id('FraudOrders') and name = 'IsPrinted') 
        alter table  dbo.FraudOrders add IsPrinted tinyint NULL 
    go

    if not exists (select * From syscolumns where id = object_id('FraudOrders') and name = 'AuthorizedOn') 
        alter table  dbo.FraudOrders add  AuthorizedOn datetime NULL
    go

    if not exists (select * From syscolumns where id = object_id('FraudOrders') and name = 'CapturedOn') 
        alter table  dbo.FraudOrders add  CapturedOn datetime NULL
    go

    if not exists (select * From syscolumns where id = object_id('FraudOrders') and name = 'RefundedOn') 
        alter table  dbo.FraudOrders add  RefundedOn datetime NULL
    go

    if not exists (select * From syscolumns where id = object_id('FraudOrders') and name = 'VoidedOn') 
        alter table  dbo.FraudOrders add  VoidedOn datetime NULL
    go

    if not exists (select * From syscolumns where id = object_id('FraudOrders') and name = 'FinalizationData') 
        alter table  dbo.FraudOrders add  FinalizationData ntext NULL
    go

    if not exists (select * From syscolumns where id = object_id('FraudOrders') and name = 'MaxMindFraudScore') 
        alter table dbo.FraudOrders add MaxMindFraudScore decimal(5,2)  NULL 
    go

    declare @cmd varchar(8000), @defaultname varchar(800)
    select @defaultname = name From sysobjects where id = (select cdefault From syscolumns where id = object_id('FraudOrders') and name = 'ShippingResidenceType')
    if @defaultname is not null begin
        select @cmd = 'alter table dbo.FraudOrders drop constraint ' + @defaultname
        EXEC (@cmd)
    end
    go


    if exists (select * From syscolumns where id = object_id('FraudOrders') and name = 'CardExtraCode') 
        alter table dbo.fraudorders drop column [CardExtraCode];
    go

    if not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('FraudOrders') and name = 'ShippedEMailSentOn')
        alter table dbo.FraudOrders add [ShippedEMailSentOn] [datetime]
    go













    if exists (select * from [dbo].syscolumns with (nolock) where id = object_id('FraudOrders_ShoppingCart') and name = 'ShippingMethod')
        alter table [dbo].FraudOrders_ShoppingCart alter column [ShippingMethod] [ntext];
    go

    if exists (select * from [dbo].syscolumns with (nolock) where id = object_id('FraudOrders_ShoppingCart') and name = 'ShippingCost')
        alter table fraudorders_shoppingcart drop column ShippingCost;
    go

    if not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('FraudOrders_ShoppingCart') and name = 'CustomerEntersPrice')
        alter table dbo.FraudOrders_ShoppingCart add [CustomerEntersPrice] [tinyint] default(0) NOT NULL;
    go

    exec sp_rename 'FraudOrders_ShoppingCart.[SubscriptionMonths]', 'SubscriptionInterval', 'COLUMN';
    go

    if exists (select * from [dbo].syscolumns with (nolock) where id = object_id('FraudOrders_ShoppingCart') and name = 'SubscriptionIntervalType')
        alter table dbo.FraudOrders_ShoppingCart add [SubscriptionIntervalType] [int] NOT NULL default(3);
    go

















    if exists (select * from [dbo].syscolumns with (nolock) where id = object_id('Orders_ShoppingCart') and name = 'ShippingMethod')
        alter table [dbo].Orders_ShoppingCart alter column [ShippingMethod] [ntext];
    go

    if not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('Orders_ShoppingCart') and name = 'CustomerEntersPrice')
        alter table dbo.Orders_ShoppingCart add [CustomerEntersPrice] [tinyint] CONSTRAINT DF_Orders_ShoppingCart_CustomerEntersPrice default(0) NOT NULL;
    go

    if not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('Orders_ShoppingCart') and name = 'CustomerEntersPricePrompt')
        alter table dbo.Orders_ShoppingCart add [CustomerEntersPricePrompt] [ntext] NULL;
    go

    if exists (select * from [dbo].syscolumns with (nolock) where id = object_id('Orders_ShoppingCart') and name = 'ShippingCost')
        alter table dbo.orders_shoppingcart drop column ShippingCost;
    go

    exec sp_rename 'Orders_ShoppingCart.[SubscriptionMonths]', 'SubscriptionInterval', 'COLUMN';
    go
    if not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('Orders_ShoppingCart') and name = 'SubscriptionIntervalType')
        alter table dbo.Orders_ShoppingCart add [SubscriptionIntervalType] [int] NOT NULL default(3);
    go


    if not exists (select * From syscolumns where id = object_id('Orders_ShoppingCart') and name = 'OrderedProductManufacturerPartNumber')
        alter table dbo.Orders_ShoppingCart add [OrderedProductManufacturerPartNumber] [nvarchar](50) NULL
    go

    if not exists (select * From syscolumns where id = object_id('Orders_ShoppingCart') and name = 'OrderedProductWeight')
        alter table dbo.Orders_ShoppingCart add [OrderedProductWeight] [money] NULL
    go

    if not exists (select * From syscolumns where id = object_id('Orders_ShoppingCart') and name = 'TaxClassID') 
        alter table dbo.Orders_ShoppingCart add [TaxClassID] [int] 
    go
    if not exists (select * From syscolumns where id = object_id('Orders_ShoppingCart') and name = 'TaxRate') 
        alter table dbo.Orders_ShoppingCart add [TaxRate] [money]
    go



    if exists (select * From syscolumns where id = object_id('Orders') and name = 'StateTaxRate')  
        DECLARE @cmd varchar(8000) 
        SET @cmd = 'UPDATE dbo.Orders_ShoppingCart SET TaxRate = isnull(o.StateTaxRate, 0) + isnull(o.CountryTaxRate, 0), TaxClassID = 1    FROM dbo.Orders_ShoppingCart os left join dbo.orders o on os.ordernumber = o.ordernumber'
        EXEC (@cmd)
    go


    alter table dbo.Orders_ShoppingCart alter column [TaxClassID] [int] NOT NULL 
    if not exists (select * from sysobjects where name = 'DF_Orders_ShoppingCart_TaxClassID' and type = 'D')
        alter table dbo.Orders_ShoppingCart add constraint DF_Orders_ShoppingCart_TaxClassID DEFAULT 1 FOR TaxClassID
    alter table dbo.Orders_ShoppingCart alter column [TaxRate] [money] NOT NULL 
    if not exists (select * from sysobjects where name = 'DF_Orders_ShoppingCart_TaxRate' and type = 'D')
        alter table dbo.Orders_ShoppingCart add constraint DF_Orders_ShoppingCart_TaxRate DEFAULT 0 FOR TaxRate

    go

    if not exists (select * From syscolumns where id = object_id('Orders_ShoppingCart') and (name = 'IsAKit' or name = 'IsAPack' or name = 'IsSystem'))
        alter table dbo.Orders_ShoppingCart add IsAKit tinyint null, IsAPack tinyint null, IsSystem tinyint null
    go




















    if exists (select * From syscolumns where id = object_id('Orders') and name = 'StateTaxRate')  
        alter table Orders drop column StateTaxRate;
    go

    if exists (select * From syscolumns where id = object_id('Orders') and name = 'CountryTaxRate')  
        alter table Orders drop column CountryTaxRate;
    go


    if not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('orders') and name = 'THUB_POSTED_TO_ACCOUNTING')
        ALTER TABLE [dbo].Orders Add 
            THUB_POSTED_TO_ACCOUNTING char(1) DEFAULT 'N',
            THUB_POSTED_DATE DateTime NULL,
            THUB_ACCOUNTING_REF char(25) NULL;
    GO

    if not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('orders') and name = 'ShippedEMailSentOn')
        alter table [dbo].orders add [ShippedEMailSentOn] [datetime] NULL;


    if exists (select * from [dbo].syscolumns with (nolock) where id = object_id('Orders') and name = 'ShippingMethod')
        alter table [dbo].Orders alter column [ShippingMethod] [ntext];
    go

    if not exists (select * From syscolumns with (nolock) where id = object_id('Orders') and name = 'RefundReason')
        ALTER TABLE dbo.Orders add RefundReason ntext;
    go

    if not exists (select * From syscolumns where id = object_id('orders') and name = 'InventoryWasReduced')
        alter table dbo.Orders add InventoryWasReduced int NOT NULL CONSTRAINT DF_Orders_InvReduced DEFAULT((0)) 
    go

    if not exists (select * From syscolumns where id = object_id('Orders') and name = 'MaxMindFraudScore') begin
        alter table dbo.Orders add [MaxMindFraudScore] decimal(5,2) NULL CONSTRAINT DF_Orders_MaxMindScore DEFAULT(-1)
    end
    else begin
        declare @defaultname nvarchar(4000)
        select @defaultname = so.name from sysobjects so join syscolumns sc on so.id = sc.cdefault where sc.id = object_id('orders') and sc.name = 'MaxMindFraudScore'
        if @defaultname is not null 
            exec ('ALTER TABLE [dbo].[Orders] DROP CONSTRAINT ' + @defaultname)
        alter table dbo.[Orders] add constraint DF_Orders_MaxMindScore DEFAULT (-1)  for MaxMindFraudScore
    end
    go





    if not exists (select * From syscolumns where id = object_id('Orders') and name = 'AuthorizedOn') 
        alter table dbo.Orders add
            [AuthorizedOn] [datetime] ,
            [CapturedOn] [datetime] , -- should generally be the same as PaymentClearedOn
            [RefundedOn] [datetime] ,
            [VoidedOn] [datetime];
    go

    if exists (select * From syscolumns where id = object_id('Orders') and name = 'CardExtraCode') 
        alter table dbo.orders drop column [CardExtraCode];
    go

    if not exists (select * From syscolumns where id = object_id('Orders') and name = 'ReadyToShip') 
        alter table dbo.orders add [ReadyToShip] tinyint default(0) NOT NULL;
    go

    if not exists (select * From syscolumns where id = object_id('Orders') and name = 'IsPrinted') 
        alter table dbo.Orders add	[IsPrinted] tinyint default(0) NOT NULL;
    go

    if not exists (select * From syscolumns where id = object_id('Orders') and name = 'Last4') 
        alter table dbo.Orders add [Last4] [nvarchar] (4) NULL;
    go


    if not exists (select * From syscolumns with (nolock) where id = object_id('Orders') and name = 'TrackingURL')
        alter table dbo.Orders add 	[TrackingURL] [ntext];
    go

    if not exists (select * From syscolumns where id = object_id('Orders') and name = 'VATRegistrationID')
        alter table dbo.Orders add [VATRegistrationID] [ntext] NULL;
    go

    if not exists (select * From syscolumns where id = object_id('Orders') and name = 'FraudedOn')
        alter table dbo.Orders add [FraudedOn] [tinyint] NULL
    go

    if not exists (select * From syscolumns where id = object_id('orders') and name = 'RecurringSubscriptionID') begin
        alter table dbo.Orders add [RecurringSubscriptionID] [nvarchar](100) NOT NULL CONSTRAINT DF_Orders_RecurringSubscriptionID DEFAULT('')
    end
    go



    if exists (select * from syscolumns where id = object_id('Orders')  and name = 'Phone' )
        alter table dbo.Orders alter column Phone nvarchar(25) null
    go


    if not exists (select * From syscolumns where id = object_id('Orders') and name = 'Crypt') begin
        alter table dbo.Orders add [Crypt] [int] NOT NULL CONSTRAINT DF_Orders_Crypt DEFAULT(1);
        declare @cmd varchar(8000) 
        set @cmd = 'update dbo.Orders set Crypt = 0'
        exec (@cmd)
    end
    go

    if not exists (select * From syscolumns where id = object_id('Orders') and name = 'CardStartDate')
       alter table dbo.Orders add 	[CardStartDate] [ntext] NULL, -- used by UK/EU customers
                                    [CardIssueNumber] [ntext] NULL -- used by UK/EU customers
    go

    if not exists (select * From syscolumns where id = object_id('orders') and name = 'TransactionType')
        -- 0 = unknown, 1 = CHARGE, 2 = CREDIT. this flag indicates how the transaction was originally created!
        -- It does NOT tell you what subsequent state the transaction is in. The TransactionState value tells you that.
        -- this flag is set ONLY ONCE when the order is originally created, so you know what type of transaction it was.
        -- in the current design, the only time it should be 2 is if it's an ad-hoc refund
        -- all other cases it should be a 1
        alter table dbo.orders add [TransactionType] [int] NOT NULL CONSTRAINT DF_Orders_TransactionType DEFAULT((0));
    go

    update orders set TransactionType=1 where TransactionType=0;
    go

    update orders set TransactionType=2 where (ParentOrderNumber IS NOT NULL and ParentOrderNumber<>0) and TransactionState='REFUNDED' and TransactionType=0
    go


    if (select isnullable from syscolumns where id = object_id('orders') and name = 'OrderWeight') = 1
        declare @defaultname nvarchar(4000)
        select @defaultname = so.name from sysobjects so join syscolumns sc on so.id = sc.cdefault where sc.id = object_id('orders') and sc.name = 'OrderWeight'
        if @defaultname is not null 
            exec ('ALTER TABLE [dbo].[Orders] DROP CONSTRAINT ' + @defaultname)
        alter table dbo.[Orders] add CONSTRAINT DF_Orders_OrderWeight DEFAULT((0.0)) for OrderWeight 
        alter table dbo.Orders ALTER COLUMN OrderWeight money NOT NULL
    go

    if not exists (select * from syscolumns where id = object_id('Orders')  and name = 'RelatedOrderNumber' )
        alter table dbo.orders add RelatedOrderNumber int not null CONSTRAINT DF_Orders_RelatedOrderNumber DEFAULT (0)
    go

    if not exists (select * From syscolumns where id = object_id('Orders') and name = 'SkinID')
        exec sp_rename 'dbo.Orders.SiteID', 'SkinID', 'COLUMN'

    go

    if not exists (select * From syscolumns where id = object_id('orders') and name = 'InventoryWasReduced')
        alter table dbo.Orders add InventoryWasReduced int NOT NULL CONSTRAINT DF_Orders_InvReduced DEFAULT((0)) 
    go

    if not exists (select * From syscolumns where id = object_id('Orders') and name = 'MaxMindDetails') 
        alter table dbo.Orders add [MaxMindDetails] ntext NULL
    go

    if exists (select * from syscolumns where id = object_id('Orders')  and name = 'Password')
        alter table dbo.orders drop column Password
    go

    if exists (select * from syscolumns where id = object_id('Orders')  and name = 'LastIPAddress' )
        alter table dbo.Orders alter column LastIPAddress varchar(40) null
    go

    if not exists (select * From syscolumns where id = object_id('Orders') and name = 'RecurringSubscriptionCommand') 
        alter table dbo.Orders add [RecurringSubscriptionCommand] [ntext] CONSTRAINT DF_Orders_RecurringSubscriptionCommand default('') NOT NULL;
    go

    if not exists (select * From syscolumns where id = object_id('Orders') and name = 'RecurringSubscriptionResult') 
        alter table dbo.Orders add [RecurringSubscriptionResult] [ntext] CONSTRAINT DF_Orders_RecurringSubscriptionResult default('') NOT NULL;
    go

    if not exists (select * From syscolumns where id = object_id('orders') and name = 'EditedOn') begin
        ALTER TABLE dbo.Orders ADD [EditedOn] [datetime] NULL
    end 
    go




    update [dbo].orders Set TransactionState='AUTHORIZED' where TransactionState='AUTH' or TransactionState='' or TransactionState IS NULL;
    go
    update [dbo].orders Set TransactionState='CAPTURED' where TransactionState='AUTH CAPTURE';
    go
    update [dbo].orders Set AuthorizedOn=OrderDate where TransactionState='AUTHORIZED';
    go
    update [dbo].orders Set CapturedOn=PaymentClearedOn where TransactionState='CAPTURED';
    go
    update [dbo].orders set VoidedOn=OrderDate, PaymentClearedOn=NULL where TransactionState='VOIDED';
    go
    update [dbo].orders set RefundedOn=OrderDate, PaymentClearedOn=NULL where TransactionState='REFUNDED';
    go



    if exists (select * From syscolumns where id = object_id('Orders') and name = 'PaymentClearedOn')
        alter table dbo.Orders drop column PaymentClearedOn
    go


    if not exists (select * From syscolumns where id = object_id('Orders') and name = 'BuySafeCommand') 
        alter table dbo.Orders add [BuySafeCommand] [ntext] CONSTRAINT DF_Orders_BuySafeCommand default('') NOT NULL;
    go

    if not exists (select * From syscolumns where id = object_id('Orders') and name = 'BuySafeResult') 
        alter table dbo.Orders add [BuySafeResult] [ntext] CONSTRAINT DF_Orders_BuySafeResult default('') NOT NULL;
    go


    if exists (select * from syscolumns where id = object_id('Orders')  and name = 'StoreVersion' )
        alter table dbo.Orders alter column StoreVersion nvarchar(100) null
    go






















    if not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('ShoppingCart') and name = 'CustomerEntersPrice')
        alter table dbo.ShoppingCart add [CustomerEntersPrice] [tinyint] default(0) NOT NULL;
    go

    if exists (select * from [dbo].syscolumns with (nolock) where id = object_id('ShoppingCart') and name = 'ShippingMethod')
        alter table [dbo].ShoppingCart alter column [ShippingMethod] [ntext];
    go

    if exists (select * from [dbo].syscolumns with (nolock) where id = object_id('ShoppingCart') and name = 'shippingcost') begin 
        declare @def varchar(255)
        select @def = so.name from [dbo].sysobjects so join [dbo].syscolumns sc on so.id = sc.cdefault and sc.id = object_id('ShoppingCart') and sc.name = 'ShippingCost'
        exec ('ALTER TABLE dbo.ShoppingCart DROP CONSTRAINT ' + @def)
    end
    go

    if exists (select * from [dbo].syscolumns with (nolock) where id = object_id('ShoppingCart') and name = 'shippingcost') 
        alter table dbo.shoppingcart drop column ShippingCost;
    go

    exec sp_rename 'ShoppingCart.[SubscriptionMonths]', 'SubscriptionInterval', 'COLUMN';
    go

    if not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('ShoppingCart') and name = 'SubscriptionIntervalType')
        alter table dbo.ShoppingCart add [SubscriptionIntervalType] [int] NOT NULL default(3);
    go

    IF not exists (select * From dbo.syscolumns where id = object_id('ShoppingCart') and name = 'RequiresCount')
        alter table [dbo].[ShoppingCart] add [RequiresCount] [int] default(0) NOT NULL;
    go

    if not exists (select * From syscolumns where id = object_id('ShoppingCart') and name = 'TaxClassID') 
        alter table dbo.ShoppingCart add [TaxClassID] [int] CONSTRAINT DF_ShoppingCart_TaxClassID DEFAULT(1)
    go
    if not exists (select * From syscolumns where id = object_id('ShoppingCart') and name = 'TaxRate') 
        alter table dbo.ShoppingCart add [TaxRate] [money] CONSTRAINT DF_ShoppingCart_TaxRate DEFAULT(0)
    go

    if not exists (select * From syscolumns where id = object_id('ShoppingCart') and name = 'RecurringSubscriptionID') 
        alter table dbo.ShoppingCart add [RecurringSubscriptionID] [nvarchar](100)  NOT NULL CONSTRAINT DF_ShoppingCart_RecurringSubscriptionID DEFAULT('')
    go


    update dbo.ShoppingCart set TaxClassID = 1 where TaxClassID is null
    update dbo.ShoppingCart set Taxrate = 0 where Taxrate is null

    alter table dbo.ShoppingCart alter column TaxClassID int not null
    alter table dbo.ShoppingCart alter column Taxrate money not null

    go


    update dbo.ShoppingCart 
    set TaxClassID = p.TaxClassID
    from dbo.ShoppingCart s join dbo.Product p on s.productID = p.productID

    GO

    if not exists (select * From syscolumns where id = object_id('ShoppingCart') and (name = 'IsAKit' or name = 'IsAPack' or name = 'IsSystem'))
        alter table dbo.ShoppingCart add IsAKit tinyint null, IsAPack tinyint null, IsSystem tinyint null
    go









    IF exists (select * from [dbo].syscolumns with (nolock) where id = object_id('RecurringShipments') and name = 'ShippingMethod')
        alter table [dbo].RecurringShipments alter column [ShippingMethod] [ntext];
    go







    IF exists (select * from [dbo].syscolumns with (nolock) where id = object_id('ShippingMethod') and name = 'Name')
        alter table [dbo].ShippingMethod alter column [Name] [nvarchar] (2000) NOT NULL;
    go

    if not exists (select * From syscolumns where id = object_id('ShippingMethod') and name = 'MappedPM') 
        alter table dbo.ShippingMethod add [MappedPM] [nvarchar](400);
    go









    if not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('ProductVariant') and name = 'CustomerEntersPrice')
        alter table dbo.ProductVariant add [CustomerEntersPrice] [tinyint] default(0) NOT NULL;
    go

    if not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('ProductVariant') and name = 'CustomerEntersPricePrompt')
        alter table dbo.ProductVariant add [CustomerEntersPricePrompt] [ntext] NULL; -- locale xml field
    go


    -- migrate their old productvariant.shippingcost values to the new table, using the FIRST NON RT SHIPPING shipping method id (we have no other way to do this):
    if exists (select * from [dbo].syscolumns with (nolock) where id = object_id('ProductVariant') and name = 'shippingcost') begin 
        DECLARE @shipmethodid int
        SELECT top 1 @shipmethodid = ShippingMethodID from [dbo].ShippingMethod where IsRTShipping = 0 ORDER BY DisplayOrder

        IF @shipmethodid is not null begin
            insert [dbo].[ShippingByProduct] (ShippingByProductGUID, VariantID, ShippingMethodID, ShippingCost, CreatedOn)
            SELECT newid(), variantid, @shipmethodid, isnull(shippingcost, 0), getdate()
            from [dbo].ProductVariant

            alter table [dbo].ProductVariant drop column shippingcost
        end
    end
    -- end migrate
    go

    exec sp_rename 'ProductVariant.[SubscriptionMonths]', 'SubscriptionInterval', 'COLUMN';
    go

    if not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('ProductVariant') and name = 'SubscriptionIntervalType')
        alter table dbo.ProductVariant add [SubscriptionIntervalType] [int] NOT NULL default(3);
    go

    if not exists (select * From syscolumns with (nolock) where id = object_id('ProductVariant') and name = 'Wholesale')
        alter table dbo.ProductVariant add [Wholesale] [tinyint] CONSTRAINT DF_ProductVariant_Wholesale default(0) NOT NULL;
    go

    if exists (select * From syscolumns where id = object_id('ProductVariant') and name = 'QuantityDiscountID')
        exec aspdnsf_DropColumnWithDefaultConstraint 'ProductVariant','QuantityDiscountID';
    go












    if exists (select * from [dbo].syscolumns with (nolock) where id = object_id('address') and name = 'CardExtraCode')
        alter table dbo.address drop column [CardExtraCode];
    go

    if not exists (select * From syscolumns where id = object_id('Address') and name = 'Crypt') begin
        alter table dbo.Address add [Crypt] [int] NOT NULL CONSTRAINT DF_Address_Crypt DEFAULT(1);
        declare @cmd varchar(8000) 
        set @cmd = 'update dbo.address set Crypt = 0'
        exec (@cmd)
    end
    go


    if not exists (select * From syscolumns where id = object_id('Address') and name = 'CardStartDate')
       alter table dbo.Address add 	[CardStartDate] [ntext] NULL, -- used by UK/EU customers
                                    [CardIssueNumber] [ntext] NULL -- used by UK/EU customers
    go







    declare @def varchar(255)
    select @def = so.name From dbo.sysobjects so join [dbo].syscolumns sc on so.id = sc.cdefault and sc.id = object_id('Distributor') and sc.name = 'NotificationTemplateNumber'
    if @@rowcount > 0 begin
        exec ('ALTER TABLE dbo.Distributor DROP CONSTRAINT ' + @def)
        ALTER TABLE dbo.Distributor DROP COLUMN NotificationTemplateNumber
    end
    go


    if not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('Distributor') and name = 'NotificationXmlPackage')
        alter table dbo.Distributor add NotificationXmlPackage [nvarchar] (100);
    go

    IF not exists (select * From dbo.syscolumns where id = object_id('Distributor') and name = 'PageSize')
        alter table dbo.Distributor add [PageSize] [int] default(20) NOT NULL;
    GO

    if not exists (select * From syscolumns with (nolock) where id = object_id('Distributor') and name = 'Wholesale')
        alter table dbo.Distributor add [Wholesale] [tinyint] CONSTRAINT DF_Distributor_Wholesale default(0) NOT NULL;
    go

    if not exists (select * from syscolumns where id = object_id('Distributor')  and name = 'TaxClassID' )
        alter table dbo.Distributor add [TaxClassID] int NOT NULL CONSTRAINT DF_Distributor_TaxClassID DEFAULT(1)
    go

    if not exists (select * from syscolumns where id = object_id('Distributor')  and name = 'SkinID' )
        alter table dbo.Distributor
        add [SkinID] int NOT NULL CONSTRAINT DF_Distributor_SkinID DEFAULT(0),
            [TemplateName] nvarchar(50) NOT NULL CONSTRAINT DF_Distributor_TemplateName DEFAULT('')
    go

    if not exists (select * From syscolumns where id = object_id('Distributor') and name = 'IsImport') 
       alter table dbo.Distributor add [IsImport] [tinyint] NOT NULL CONSTRAINT DF_Distributor_IsImport DEFAULT((0))
    go















    if (select cdefault from syscolumns where id = object_id('ProductDistributor') and name = 'DisplayOrder') = 0 begin
        alter table dbo.ProductDistributor add constraint DF_ProductDistributor_DisplayOrder DEFAULT (1)  for DisplayOrder
    end
    go













    if not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('ProductAffiliate') and name = 'DisplayOrder')
        alter table dbo.ProductAffiliate add [DisplayOrder] [int] CONSTRAINT DF_ProductAffiliate_DisplayOrder default(1) NOT NULL;
    go

    if not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('ProductCustomerLevel') and name = 'DisplayOrder')
        alter table dbo.ProductCustomerLevel add [DisplayOrder] [int] CONSTRAINT DF_ProductCustomerLevel_DisplayOrder default(1) NOT NULL;
    go

    if not exists (select * from [dbo].syscolumns with (nolock) where id = object_id('ProductLocaleSetting') and name = 'DisplayOrder')
        alter table dbo.ProductLocaleSetting add [DisplayOrder] [int] CONSTRAINT DF_ProductLocaleSetting_DisplayOrder default(1) NOT NULL;
    go






    IF not exists (select * From dbo.syscolumns where id = object_id('Category') and name = 'PageSize')
        alter table dbo.Category add [PageSize] [int] default(20) NOT NULL;
    GO

    if not exists (select * From syscolumns with (nolock) where id = object_id('Category') and name = 'Wholesale')
        alter table dbo.Category add [Wholesale] [tinyint] CONSTRAINT DF_Category_Wholesale default(0) NOT NULL;
    go

    IF not exists (select * From syscolumns where id = object_id('Category') and name = 'TaxClassID')  
        alter table dbo.Category add [TaxClassID] int NOT NULL  CONSTRAINT DF_Category_TaxClassID DEFAULT(1);
    go

    if not exists (select * from syscolumns where id = object_id('Category')  and name = 'SkinID' )
        alter table dbo.Category 
        add [SkinID] int NOT NULL CONSTRAINT DF_Category_SkinID DEFAULT(0),
            [TemplateName] nvarchar(50) NOT NULL CONSTRAINT DF_Category_TemplateName DEFAULT('')
    go






    IF not exists (select * From dbo.syscolumns where id = object_id('Section') and name = 'PageSize')
        alter table dbo.Section add [PageSize] [int] default(20) NOT NULL;
    GO

    if not exists (select * From syscolumns with (nolock) where id = object_id('Section') and name = 'Wholesale')
        alter table dbo.Section add [Wholesale] [tinyint] CONSTRAINT DF_Section_Wholesale default(0) NOT NULL;
    go

    if not exists (select * from syscolumns where id = object_id('Section')  and name = 'SkinID' )
        alter table dbo.Section 
        add [SkinID] int NOT NULL CONSTRAINT DF_Section_SkinID DEFAULT(0),
            [TemplateName] nvarchar(50) NOT NULL CONSTRAINT DF_Section_TemplateName DEFAULT('')
    go








    if not exists (select * from syscolumns where id = object_id('Vector')  and name = 'SkinID' )
        alter table dbo.Vector 
        add [SkinID] int NOT NULL CONSTRAINT DF_Vector_SkinID DEFAULT(0),
            [TemplateName] nvarchar(50) NOT NULL CONSTRAINT DF_Vector_TemplateName DEFAULT('')
    go







    if not exists (select * from syscolumns where id = object_id('Genre')  and name = 'SkinID' )
        alter table dbo.Genre 
        add [SkinID] int NOT NULL CONSTRAINT DF_Genre_SkinID DEFAULT(0),
            [TemplateName] nvarchar(50) NOT NULL CONSTRAINT DF_Genre_TemplateName DEFAULT('')
    go










    IF not exists (select * From dbo.syscolumns where id = object_id('Manufacturer') and name = 'PageSize')
        alter table dbo.Manufacturer add [PageSize] [int] default(20) NOT NULL;
    GO

    if not exists (select * From syscolumns with (nolock) where id = object_id('Manufacturer') and name = 'Wholesale')
        alter table dbo.Manufacturer add [Wholesale] [tinyint] CONSTRAINT DF_Manufacturer_Wholesale default(0) NOT NULL;
    go

    if not exists (select * from syscolumns where id = object_id('Manufacturer')  and name = 'SkinID' )
        alter table dbo.Manufacturer
        add [SkinID] int NOT NULL CONSTRAINT DF_Manufacturer_SkinID DEFAULT(0),
            [TemplateName] nvarchar(50) NOT NULL CONSTRAINT DF_Manufacturer_TemplateName DEFAULT('')
    go









    IF not exists (select * From dbo.syscolumns where id = object_id('Library') and name = 'PageSize')
        alter table dbo.Library add [PageSize] [int] default(20) NOT NULL;
    GO

    IF not exists (select * From dbo.syscolumns where id = object_id('Document') and name = 'PageSize')
        alter table dbo.Document add [PageSize] [int] default(20) NOT NULL;
    GO

    if not exists (select * From syscolumns with (nolock) where id = object_id('Document') and name = 'Wholesale')
        alter table dbo.Document add [Wholesale] [tinyint] CONSTRAINT DF_Document_Wholesale default(0) NOT NULL;
    go

    if not exists (select * from syscolumns where id = object_id('Library')  and name = 'QuantityDiscountID' )
        alter table dbo.Library add [QuantityDiscountID] int  NULL 
    go








    if not exists (select * From syscolumns with (nolock) where id = object_id('Affiliate') and name = 'Wholesale')
        alter table dbo.Affiliate add [Wholesale] [tinyint] CONSTRAINT DF_Affiliate_Wholesale default(0) NOT NULL;
    go

    if not exists (select * from syscolumns where id = object_id('Affiliate')  and name = 'SkinID' )
        alter table dbo.Affiliate 
        add [SkinID] int NOT NULL CONSTRAINT DF_Affiliate_SkinID DEFAULT(0),
            [TemplateName] nvarchar(50) NOT NULL CONSTRAINT DF_Affiliate_TemplateName DEFAULT('')
    go

    alter table dbo.Affiliate alter column Password [nvarchar] (250);
    go

    if exists (select * From syscolumns where id = object_id('Affiliate') and name = 'TaxIDNumber') begin
        alter table dbo.Affiliate drop column TaxIDNumber;
        alter table dbo.Affiliate drop column TaxRegistrationName;
        alter table dbo.Affiliate drop column TaxClassification;
    end
    go

    if not exists (select * From syscolumns where id = object_id('affiliate') and name = 'SaltKey') begin
        alter table dbo.affiliate add SaltKey int
        declare @cmd varchar(8000) 
        set @cmd = 'update dbo.affiliate set SaltKey = -1'
        exec (@cmd)

        alter table dbo.affiliate alter column [SaltKey] [int] NOT NULL 

        alter table dbo.affiliate add constraint DF_Affiliate_SaltKey DEFAULT 0 FOR SaltKey
    end
    go

    if not exists (select * from sysobjects so join syscolumns sc on so.id = sc.cdefault and sc.id = object_id('affiliate') and sc.name = 'deleted')
        alter table affiliate add CONSTRAINT DF_affiliate_deleted default(0) FOR deleted
        UPDATE affiliate set deleted = 0 where deleted is null
        alter table affiliate alter column deleted tinyint NOT NULL 
    go

    if exists (select * From sysobjects where name = 'PK_Affiliates' and type = 'K') begin
        alter table Affiliate drop constraint PK_Affiliates
        alter table Affiliate add constraint PK_Affiliate primary key (AffiliateID)
    end 


    go 








    if not exists (select * from syscolumns where id = object_id('CustomerLevel')  and name = 'SkinID' )
        alter table dbo.CustomerLevel
        add [SkinID] int NOT NULL CONSTRAINT DF_CustomerLevel_SkinID DEFAULT(0),
            [TemplateName] nvarchar(50) NOT NULL CONSTRAINT DF_CustomerLevel_TemplateName DEFAULT('')
    go

    if not exists (select * From syscolumns where id = object_id('CustomerLevel') and name = 'LevelAllowsPO')
        alter table dbo.CustomerLevel add [LevelAllowsPO] [tinyint] NOT NULL CONSTRAINT DF_CustomerLevel_LevelAllowsPO DEFAULT((1));
    go










    if not exists (select * From syscolumns with (nolock) where id = object_id('Library') and name = 'Wholesale')
        alter table dbo.Library add [Wholesale] [tinyint] CONSTRAINT DF_Library_Wholesale default(0) NOT NULL;
    go

    if not exists (select * from syscolumns where id = object_id('Library')  and name = 'SkinID' )
        alter table dbo.Library 
        add [SkinID] int NOT NULL CONSTRAINT DF_Library_SkinID DEFAULT(0),
            [TemplateName] nvarchar(50) NOT NULL CONSTRAINT DF_Library_TemplateName DEFAULT('')
    go









    if not exists (select * From syscolumns with (nolock) where id = object_id('FAQ') and name = 'Wholesale')
        alter table dbo.FAQ add [Wholesale] [tinyint] CONSTRAINT DF_FAQ_Wholesale default(0) NOT NULL;
    go




    if not exists (select * From syscolumns with (nolock) where id = object_id('News') and name = 'Wholesale')
        alter table dbo.News add [Wholesale] [tinyint] CONSTRAINT DF_New_Wholesale default(0) NOT NULL;
    go

    if not exists (select * From syscolumns with (nolock) where id = object_id('Partner') and name = 'Wholesale')
        alter table dbo.Partner add [Wholesale] [tinyint] CONSTRAINT DF_Partner_Wholesale default(0) NOT NULL;
    go

    if not exists (select * From syscolumns with (nolock) where id = object_id('Poll') and name = 'Wholesale')
        alter table dbo.Poll add [Wholesale] [tinyint] CONSTRAINT DF_Poll_Wholesale default(0) NOT NULL;
    go


    if not exists (select * From syscolumns with (nolock) where id = object_id('Staff') and name = 'Wholesale')
        alter table dbo.Staff add [Wholesale] [tinyint] CONSTRAINT DF_Staff_Wholesale default(0) NOT NULL;
    go








    if not exists (select * From syscolumns with (nolock) where id = object_id('LocaleSetting') and name = 'DefaultCurrencyID')
        alter table dbo.LocaleSetting add [DefaultCurrencyID] [int] default(1) NOT NULL;
    go








    if not exists (select * from syscolumns where id = object_id('Inventory')  and name = 'VendorFullSKU' ) begin
        alter table dbo.Inventory add [VendorFullSKU] nvarchar(50) null
    end
    go

    if not exists (select * from syscolumns where id = object_id('Inventory')  and name = 'VendorID' ) begin
        alter table dbo.Inventory add  [VendorID] nvarchar(50) null
    end
    go

    if not exists (select * from syscolumns where id = object_id('Inventory')  and name = 'ExtensionData' ) begin
        alter table dbo.Inventory add[ExtensionData] [ntext] NULL
    end
    go

    if not exists (select * From syscolumns with (nolock) where id = object_id('Inventory') and name = 'WarehouseLocation')
        ALTER TABLE dbo.Inventory ADD WarehouseLocation nvarchar(100) NULL;
    go

    if not exists (select * from syscolumns where id = object_id('Inventory')  and name = 'WeightDelta' )
        alter table dbo.Inventory add [WeightDelta] [money] NOT NULL CONSTRAINT DF_Inventory_WeightDelta DEFAULT (0.0)
    go













    if not exists (select * from syscolumns where id = object_id('ExtendedPrice')  and name = 'ExtensionData' )
        alter table dbo.ExtendedPrice add ExtensionData ntext null
    go

    if exists (select * from syscolumns where id = object_id('ExtendedPrice')  and name = 'CreatedOn' and cdefault = 0)
        alter table dbo.ExtendedPrice add constraint DF_ExtendedPrice_CreatedOn  DEFAULT (getdate()) FOR CreatedOn
    go








    if not exists (select * From syscolumns where id = object_id('ProductType') and name = 'TaxClassID')
       alter table dbo.ProductType add [TaxClassID] [int] NOT NULL CONSTRAINT DF_ProductType_TaxClassID DEFAULT((1))
    go






    if not exists (select * From syscolumns where id = object_id('StringResource') and name = 'Modified')
        alter table dbo.StringResource add [Modified] [tinyint] NOT NULL CONSTRAINT DF_StringResource_Modified DEFAULT((0)) -- if one, that means the user edited this string in the admin site
    go







    if not exists (select * From syscolumns where id = object_id('Address') and name = 'PaymentMethodLastUsed')
        exec sp_rename 'dbo.Address.PaymentMethod', 'PaymentMethodLastUsed', 'COLUMN'    
    go






    if not exists (select * from syscolumns where id = object_id('QuantityDiscount')  and name = 'DiscountType' )
        ALTER TABLE [dbo].[QuantityDiscount] ADD [DiscountType] [tinyint] NOT NULL CONSTRAINT [DF_QuantityDiscount_DiscountType] DEFAULT ((0))
    go











    -- rename default column constraints
    DECLARE @tmpalterconstraint table (cmd varchar(1000))
    insert @tmpalterconstraint 
    select ' ALTER TABLE ' + table_name + ' DROP CONSTRAINT ' + so2.name + 
        ' ALTER TABLE ' + table_name  + ' ADD  CONSTRAINT DF_' + table_name  + '_' + column_name + ' DEFAULT ' + sc.column_default + ' FOR ' + column_name 

    FROM information_schema.columns sc 
        join sysobjects so on sc.table_name = so.name and so.type = 'u' 
        join syscolumns sc2 on sc.column_name = sc2.name and sc2.id = so.id
        join sysobjects so2 on sc2.cdefault = so2.id
    where sc.column_default is not null


    declare @cmd varchar(8000)

    SELECT top 1 @cmd = cmd from @tmpalterconstraint
    while @@rowcount > 0 begin
        exec (@cmd)
        delete @tmpalterconstraint where cmd = @cmd 
        SELECT top 1 @cmd = cmd from @tmpalterconstraint
    end
    go




/*****************************************  Indexes  ******************************************/


    -- Drop All Unique Constraints
    DECLARE @tmpaltertable table (cmd varchar(1000))
    insert @tmpaltertable
    select 'ALTER TABLE ' + so2.name + ' DROP CONSTRAINT ' + so.name
    FROM sysobjects so join sysobjects so2 on so.parent_obj = so2.id
    where so.xtype = 'UQ' 
    order by so.name

    declare @cmd varchar(8000)

    SELECT top 1 @cmd = cmd from @tmpaltertable
    while @@rowcount > 0 begin
        exec (@cmd)
        delete @tmpaltertable where cmd = @cmd 
        SELECT top 1 @cmd = cmd from @tmpaltertable
    end
    go


    if not exists (select * from sysindexes where name = 'IX_GiftCard_CreatedOn' and id = object_id('GiftCard'))
        CREATE NONCLUSTERED INDEX [IX_GiftCard_CreatedOn] ON [dbo].[GiftCard] ([CreatedOn])
    go

    if not exists (select * from sysindexes where name = 'IX_GiftCard_ExpirationDate' and id = object_id('GiftCard'))
        CREATE NONCLUSTERED INDEX [IX_GiftCard_ExpirationDate] ON [dbo].[GiftCard] ([ExpirationDate])
    GO
    if not exists (select * from sysindexes where name = 'IX_GiftCard_PurchasedByCustomerID' and id = object_id('GiftCard'))
        CREATE NONCLUSTERED INDEX [IX_GiftCard_PurchasedByCustomerID] ON [dbo].[GiftCard] ([PurchasedByCustomerID])
    GO
    if not exists (select * from sysindexes where name = 'IX_GiftCard_SerialNumber' and id = object_id('GiftCard'))
        CREATE NONCLUSTERED INDEX [IX_GiftCard_SerialNumber] ON [dbo].[GiftCard] ([SerialNumber])
    GO
    if not exists (select * from sysindexes where name = 'UIX_GiftCard_GiftCardGUID' and id = object_id('GiftCard'))
        CREATE UNIQUE NONCLUSTERED INDEX [UIX_GiftCard_GiftCardGUID] ON [dbo].[GiftCard] ([GiftCardGUID])
    GO

    if not exists (select * from sysindexes where name = 'IX_GiftCardUsage_GiftCardID' and id = object_id('GiftCardUsage'))
        CREATE NONCLUSTERED INDEX [IX_GiftCardUsage_GiftCardID] ON [dbo].[GiftCardUsage] ([GiftCardID])
    GO
    if not exists (select * from sysindexes where name = 'IX_GiftCardUsage_UsedByCustomerID' and id = object_id('GiftCardUsage'))
        CREATE NONCLUSTERED INDEX [IX_GiftCardUsage_UsedByCustomerID] ON [dbo].[GiftCardUsage] ([UsedByCustomerID])
    GO



    if not exists (select * from sysindexes where name = 'UIX_Coupon_CouponGUID' and id = object_id('Coupon'))
        CREATE UNIQUE INDEX [UIX_Coupon_CouponGUID] ON [Coupon]([CouponGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_Coupon_CouponCode' and id = object_id('Coupon'))
        CREATE UNIQUE INDEX [UIX_Coupon_CouponCode] ON [Coupon]([CouponCode]);
    go

    if not exists (select * from sysindexes where name = 'UIX_CreditCardTypes' and id = object_id('CreditCardType'))
        CREATE UNIQUE NONCLUSTERED INDEX [UIX_CreditCardTypes] ON [dbo].[CreditCardType] ([CardTypeGUID])
    go


    if not exists (select * from sysindexes where name = 'UIX_Currency_CurrencyGUID' and id = object_id('Currency'))
        CREATE UNIQUE NONCLUSTERED INDEX [UIX_Currency_CurrencyGUID] ON [dbo].[Currency] ([CurrencyGUID])
    go

    if not exists (select * from sysindexes where name = 'IX_Feed_DisplayOrder' and id = object_id('Feed'))
        CREATE INDEX [IX_Feed_DisplayOrder] ON [dbo].[Feed]([DisplayOrder]) ;
    go

    if not exists (select * from sysindexes where name = 'IX_Feed_DisplayOrder_Name' and id = object_id('Feed'))
        CREATE INDEX [IX_Feed_DisplayOrder_Name] ON [dbo].[Feed]([DisplayOrder],[Name]) ;
    go

    if not exists (select * from sysindexes where name = 'IX_Orders_TransactionState' and id = object_id('Feed'))
        CREATE INDEX [IX_Orders_TransactionState] ON [Orders]([TransactionState]) ;
    go


    if not exists (select * from sysindexes where name = 'IX_Category_Wholesale' and id = object_id('Category'))
        CREATE INDEX [IX_Category_Wholesale] ON [dbo].[Category]([Wholesale]) ;
    go

    if not exists (select * from sysindexes where name = 'IX_Category_Deleted_Wholesale' and id = object_id('Category'))
        CREATE INDEX [IX_Category_Deleted_Wholesale] ON [dbo].[Category]([Deleted],[Wholesale]) ;
    go

    if not exists (select * from sysindexes where name = 'IX_Document_Wholesale' and id = object_id('Document'))
        CREATE INDEX [IX_Document_Wholesale] ON [dbo].[Document]([Wholesale]) ;
    go

    if not exists (select * from sysindexes where name = 'IX_Document_Wholesale_Deleted' and id = object_id('Document'))
        CREATE INDEX [IX_Document_Wholesale_Deleted] ON [dbo].[Document]([Wholesale],[Deleted]) ;
    go

    if not exists (select * from sysindexes where name = 'IX_Library_Wholesale' and id = object_id('Library'))
        CREATE INDEX [IX_Library_Wholesale] ON [dbo].[Library]([Wholesale]) ;
    go

    if not exists (select * from sysindexes where name = 'IX_Library_Deleted_Wholesale' and id = object_id('Library'))
        CREATE INDEX [IX_Library_Deleted_Wholesale] ON [dbo].[Library]([Deleted],[Wholesale]) ;
    go

    if not exists (select * from sysindexes where name = 'IX_News_Wholesale' and id = object_id('News'))
        CREATE INDEX [IX_News_Wholesale] ON [dbo].[News]([Wholesale]) ;
    go

    if not exists (select * from sysindexes where name = 'IX_Partner_Wholesale' and id = object_id('Partner'))
        CREATE INDEX [IX_Partner_Wholesale] ON [dbo].[Partner]([Wholesale]) ;
    go

    if not exists (select * from sysindexes where name = 'IX_Poll_Wholesale' and id = object_id('Poll'))
        CREATE INDEX [IX_Poll_Wholesale] ON [dbo].[Poll]([Wholesale]) ;
    go

    if not exists (select * from sysindexes where name = 'IX_Product_Wholesale' and id = object_id('Product'))
        CREATE INDEX [IX_Product_Wholesale] ON [dbo].[Product]([Wholesale]) ;
    go

    if not exists (select * from sysindexes where name = 'IX_Product_Wholesale_Deleted' and id = object_id('Product'))
        CREATE INDEX [IX_Product_Wholesale_Deleted] ON [dbo].[Product]([Wholesale],[Deleted]) ;
    go

    if not exists (select * from sysindexes where name = 'IX_ProductVariant_Wholesale' and id = object_id('ProductVariant'))
        CREATE INDEX [IX_ProductVariant_Wholesale] ON [dbo].[ProductVariant]([Wholesale]) ;
    go

    if not exists (select * from sysindexes where name = 'IX_ProductVariant_Deleted_Wholesale' and id = object_id('ProductVariant'))
        CREATE INDEX [IX_ProductVariant_Deleted_Wholesale] ON [dbo].[ProductVariant]([Deleted],[Wholesale]) ;
    go

    if not exists (select * from sysindexes where name = 'IX_Section_Wholesale' and id = object_id('Section'))
        CREATE INDEX [IX_Section_Wholesale] ON [dbo].[Section]([Wholesale]) ;
    go

    if not exists (select * from sysindexes where name = 'IX_Section_Deleted_Wholesale' and id = object_id('Section'))
        CREATE INDEX [IX_Section_Deleted_Wholesale] ON [dbo].[Section]([Deleted],[Wholesale]) ;
    go

    if not exists (select * from sysindexes where name = 'IX_Staff_Wholesale' and id = object_id('Staff'))
        CREATE INDEX [IX_Staff_Wholesale] ON [dbo].[Staff]([Wholesale]) ;
    go



    if exists (select * from sysindexes where name = 'IX_State_Abbreviation')
        DROP INDEX State.IX_State_Abbreviation
    GO


    if not exists (select * from sysindexes where name = 'UIX_Address_AddressGUID' and id = object_id('Address'))
        CREATE UNIQUE INDEX [UIX_Address_AddressGUID] ON [Address] ([AddressGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_AppConfig_Name' and id = object_id('AppConfig'))
        CREATE UNIQUE INDEX [UIX_AppConfig_Name] ON [AppConfig]([Name]);
    go

    if not exists (select * from sysindexes where name = 'UIX_Country_Name' and id = object_id('Country'))
        CREATE UNIQUE INDEX [UIX_Country_Name] ON [Country]([Name]);
    go

    if not exists (select * from sysindexes where name = 'UIX_AppConfig_Name' and id = object_id('AppConfig'))
        CREATE UNIQUE INDEX [UIX_Coupon_CouponGUID] ON [Coupon]([CouponGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_AppConfig_Name' and id = object_id('AppConfig'))
        CREATE UNIQUE INDEX [UIX_Coupon_CouponCode] ON [Coupon]([CouponCode]);
    go

    if not exists (select * from sysindexes where name = 'UIX_AppConfig_Name' and id = object_id('AppConfig'))
        CREATE UNIQUE INDEX [UIX_CreditCardTypes] ON [CreditCardType]([CardTypeGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_AppConfig_Name' and id = object_id('AppConfig'))
        CREATE UNIQUE INDEX [UIX_Currency_CurrencyGUID] ON [Currency]([CurrencyGUID])
    go

    if not exists (select * from sysindexes where name = 'UIX_Customer_CustomerGUID' and id = object_id('Customer'))
        CREATE UNIQUE INDEX [UIX_Customer_CustomerGUID] ON [Customer]([CustomerGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_CustomerGiftRegistrySearches_CustomerID_GIftRegistryGUID' and id = object_id('CustomerGiftRegistrySearches'))
        CREATE UNIQUE INDEX [UIX_CustomerGiftRegistrySearches_CustomerID_GIftRegistryGUID] ON [CustomerGiftRegistrySearches] ([CustomerID],[GIftRegistryGUID]) ;
    go

    if not exists (select * from sysindexes where name = 'UIX_Distributor_DistributorGUID' and id = object_id('Distributor'))
        CREATE UNIQUE INDEX [UIX_Distributor_DistributorGUID] ON [Distributor]([DistributorGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_Document_DocumentGUID' and id = object_id('Document'))
        CREATE UNIQUE INDEX [UIX_Document_DocumentGUID] ON [Document]([DocumentGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_DocumentLibrary_DocumentID_LibraryID' and id = object_id('DocumentLibrary'))
        CREATE UNIQUE INDEX [UIX_DocumentLibrary_DocumentID_LibraryID] ON [DocumentLibrary]([DocumentID],[LibraryID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_Expense' and id = object_id('Expense'))
        CREATE UNIQUE INDEX [UIX_Expense] ON [Expense]([ExpenseGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_ExpenseCategory_ExpenseCategoryGUID' and id = object_id('ExpenseCategory'))
        CREATE UNIQUE INDEX [UIX_ExpenseCategory_ExpenseCategoryGUID] ON ExpenseCategory ([ExpenseCategoryGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_ExtendedPrice_2' and id = object_id('ExtendedPrice'))
        CREATE UNIQUE INDEX [UIX_ExtendedPrice_2] ON [ExtendedPrice] ([ExtendedPriceGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_Gallery_GalleryGUID' and id = object_id('Gallery'))
        CREATE UNIQUE INDEX [UIX_Gallery_GalleryGUID] ON [Gallery] ([GalleryGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_Feed_FeedGUID' and id = object_id('Feed'))
        CREATE UNIQUE INDEX [UIX_Feed_FeedGUID] ON [Feed] ([FeedGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_GiftCard_GiftCardGUID' and id = object_id('GiftCard'))
        CREATE UNIQUE INDEX [UIX_GiftCard_GiftCardGUID] ON [GiftCard] ([GiftCardGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_GiftCardUsage_GiftCardUsageGUID' and id = object_id('GiftCardUsage'))
        CREATE UNIQUE INDEX [UIX_GiftCardUsage_GiftCardUsageGUID] ON [GiftCardUsage]([GiftCardUsageGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_Inventory_InventoryGUID' and id = object_id('Inventory'))
        CREATE UNIQUE INDEX [UIX_Inventory_InventoryGUID] ON [Inventory] ([InventoryGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_KitGroup_KitGroupGUID' and id = object_id('KitGroup'))
        CREATE UNIQUE INDEX [UIX_KitGroup_KitGroupGUID] ON [KitGroup] ([KitGroupGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_KitGroupType_KitGroupTypeGUID' and id = object_id('KitGroupType'))
        CREATE UNIQUE INDEX [UIX_KitGroupType_KitGroupTypeGUID] ON [KitGroupType] ([KitGroupTypeGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_KitItem_KitItemGUID' and id = object_id('KitItem'))
        CREATE UNIQUE INDEX [UIX_KitItem_KitItemGUID] ON [KitItem] ([KitItemGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_Locale_LocaleSettingGUID' and id = object_id('LocaleSetting'))
        CREATE UNIQUE INDEX [UIX_Locale_LocaleSettingGUID] ON [LocaleSetting] ([LocaleSettingGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_Locale_Name' and id = object_id('LocaleSetting'))
        CREATE UNIQUE INDEX [UIX_Locale_Name] ON [LocaleSetting] ([Name]);
    go

    if not exists (select * from sysindexes where name = 'UIX_MailingMgrLog_MailingMgrLogGUID' and id = object_id('MailingMgrLog'))
        CREATE UNIQUE INDEX [UIX_MailingMgrLog_MailingMgrLogGUID] ON [MailingMgrLog] ([MailingMgrLogGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_Manufacturer_ManufacturerGUID' and id = object_id('Manufacturer'))
        CREATE UNIQUE INDEX [UIX_Manufacturer_ManufacturerGUID] ON [Manufacturer] ([ManufacturerGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_News_NewsGUID' and id = object_id('News'))
        CREATE UNIQUE INDEX [UIX_News_NewsGUID] ON [News] ([NewsGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_OrderNumbers_OrderNumberGUID' and id = object_id('OrderNumbers'))
        CREATE UNIQUE INDEX [UIX_OrderNumbers_OrderNumberGUID] ON [OrderNumbers] ([OrderNumberGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_Partner_PartnerGUID' and id = object_id('Partner'))
        CREATE UNIQUE INDEX [UIX_Partner_PartnerGUID] ON [Partner] ([PartnerGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_Poll_PollGUID' and id = object_id('Poll'))
        CREATE UNIQUE INDEX [UIX_Poll_PollGUID] ON [Poll] ([PollGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_PollAnswer_PollAnswerGUID' and id = object_id('PollAnswer'))
        CREATE UNIQUE INDEX [UIX_PollAnswer_PollAnswerGUID] ON [PollAnswer] ([PollAnswerGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_PollSortOrder_PollSortOrderGUID' and id = object_id('PollSortOrder'))
        CREATE UNIQUE INDEX [UIX_PollSortOrder_PollSortOrderGUID] ON [PollSortOrder] ([PollSortOrderGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_PollVotingRecord_PollID_CustomerID' and id = object_id('PollVotingRecord'))
        CREATE UNIQUE INDEX [UIX_PollVotingRecord_PollID_CustomerID] ON [PollVotingRecord] ([PollID],[CustomerID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_PollVotingRecord_PollVotingRecordGUID' and id = object_id('PollVotingRecord'))
        CREATE UNIQUE INDEX [UIX_PollVotingRecord_PollVotingRecordGUID] ON [PollVotingRecord] ([PollVotingRecordGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_Product_ProductGUID' and id = object_id('Product'))
        CREATE UNIQUE INDEX [UIX_Product_ProductGUID] ON [Product] ([ProductGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_ProductVariant_VariantID' and id = object_id('ProductVariant'))
        CREATE UNIQUE INDEX [UIX_ProductVariant_VariantID] ON [ProductVariant] ([VariantGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_QuantityDiscount_QuantityDiscountGUID' and id = object_id('QuantityDiscount'))
        CREATE UNIQUE INDEX [UIX_QuantityDiscount_QuantityDiscountGUID] ON [QuantityDiscount] ([QuantityDiscountGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_QuantityDiscountTable_QuantityDiscountTableGUID' and id = object_id('QuantityDiscountTable'))
        CREATE UNIQUE INDEX [UIX_QuantityDiscountTable_QuantityDiscountTableGUID] ON [QuantityDiscountTable] ([QuantityDiscountTableGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_RecurringShipments_RecurringShipmentsGUID' and id = object_id('RecurringShipments'))
        CREATE UNIQUE INDEX [UIX_RecurringShipments_RecurringShipmentsGUID] ON [RecurringShipments] ([RecurringShipmentsGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_SkinPreview_Name' and id = object_id('AppConfig'))
        CREATE UNIQUE INDEX [UIX_SkinPreview_Name] ON [SkinPreview] ([Name]);
    go

    if not exists (select * from sysindexes where name = 'UIX_SalesPrompt_SalesPromptGUID' and id = object_id('SalesPrompt'))
        CREATE UNIQUE INDEX [UIX_SalesPrompt_SalesPromptGUID] ON [SalesPrompt] ([SalesPromptGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_Section_SectionGUID' and id = object_id('Section'))
        CREATE UNIQUE INDEX [UIX_Section_SectionGUID] ON [Section] ([SectionGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_ShippingByProduct_ShippingByProductGUID' and id = object_id('ShippingByProduct'))
        CREATE UNIQUE INDEX [UIX_ShippingByProduct_ShippingByProductGUID] ON [dbo].[ShippingByProduct]([ShippingByProductGUID])
    go

    if not exists (select * from sysindexes where name = 'UIX_ShippingCalculation_ShippingCalculationID' and id = object_id('ShippingCalculation'))
        CREATE UNIQUE INDEX [UIX_ShippingCalculation_ShippingCalculationID] ON [ShippingCalculation] ([ShippingCalculationGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_ShippingMethod_ShippingMethodGUID' and id = object_id('ShippingMethod'))
        CREATE UNIQUE INDEX [UIX_ShippingMethod_ShippingMethodGUID] ON [ShippingMethod] ([ShippingMethodGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_ShippingZone_ShippingZoneGUID' and id = object_id('ShippingZone'))
        CREATE UNIQUE INDEX [UIX_ShippingZone_ShippingZoneGUID] ON [ShippingZone] ([ShippingZoneGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_ShoppingCart_ShoppingCartRecGUID' and id = object_id('ShoppingCart'))
        CREATE UNIQUE INDEX [UIX_ShoppingCart_ShoppingCartRecGUID] ON [ShoppingCart] ([ShoppingCartRecGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_Staff_StaffGUID' and id = object_id('Staff'))
        CREATE UNIQUE INDEX [UIX_Staff_StaffGUID] ON [Staff] ([StaffGUID]);
    go

    if not exists (select * from sysindexes where name = 'UIX_State_StateGUID' and id = object_id('State'))
        CREATE UNIQUE INDEX [UIX_State_StateGUID] ON [State] ([StateGUID]);
    go


    if not exists (select * from sysindexes where name = 'UIX_State_Abbreviation' and id = object_id('State'))
        CREATE UNIQUE INDEX [UIX_State_Abbreviation] ON [dbo].[State]([Abbreviation])
    go

    if not exists (select * from sysindexes where name = 'UIX_StringResource_Name_LocaleSetting' and id = object_id('StringResource'))
        CREATE UNIQUE INDEX [UIX_StringResource_Name_LocaleSetting] ON [StringResource]([Name],[LocaleSetting]);
    go

    if not exists (select * from sysindexes where name = 'UIX_Topic_TopicGUID' and id = object_id('Topic'))
        CREATE UNIQUE INDEX [UIX_Topic_TopicGUID] ON [Topic] ([TopicGUID]);
    GO

    if not exists (select * from sysindexes where name = 'IX_ShippingByTotal_ShippingMethodID_LowValue_HighValue')
        CREATE INDEX [IX_ShippingByTotal_ShippingMethodID_LowValue_HighValue] ON [ShippingByTotal]([ShippingMethodID],[LowValue],[HighValue]) ;
    go

    if not exists (select * from sysindexes where name = 'IX_ShippingByWeight_ShippingMethodID_LowValue_HighValue')
        CREATE INDEX [IX_ShippingByWeight_ShippingMethodID_LowValue_HighValue] ON [ShippingByWeight]([ShippingMethodID],[LowValue],[HighValue]) ;
    go

    if not exists (select * from sysindexes where name = 'IX_ShippingWeightByZone_ShippingZoneID_LowValue_HighValue')
        CREATE INDEX [IX_ShippingWeightByZone_ShippingZoneID_LowValue_HighValue] ON [ShippingWeightByZone]([ShippingZoneID],[LowValue],[HighValue]) ;
    go


    if not exists (select * from sysindexes where name = 'IX_ProductSection_SectionID_DisplayOrder')
        CREATE INDEX [IX_ProductSection_SectionID_DisplayOrder] ON [ProductSection]([SectionID],[DisplayOrder]) ;
    go




    if not exists (select * from sysindexes where name = 'IX_ShoppingCart_CartType_RecurringSubscriptionID' and id = object_id('ShoppingCart'))
        CREATE INDEX [IX_ShoppingCart_CartType_RecurringSubscriptionID] ON [ShoppingCart]([CartType], [RecurringSubscriptionID]);

    if not exists (select * from sysindexes where name = 'UIX_Genre_GenreGUID' and id = object_id('Genre'))
        CREATE UNIQUE INDEX [UIX_Genre_GenreGUID] ON [Genre]([GenreGUID]);

    if not exists (select * from sysindexes where name = 'IX_Genre_Name' and id = object_id('Genre'))
        CREATE INDEX [IX_Genre_Name] ON [Genre]([Name]) ;

    if not exists (select * from sysindexes where name = 'IX_Genre_DisplayOrder_Name' and id = object_id('Genre'))
        CREATE INDEX [IX_Genre_DisplayOrder_Name] ON [Genre]([DisplayOrder],[Name]) ;
    go

    if not exists (select * from sysindexes where name = 'UIX_Vector_VectorGUID' and id = object_id('Vector'))
        CREATE UNIQUE INDEX [UIX_Vector_VectorGUID] ON [Vector]([VectorGUID]);
    if not exists (select * from sysindexes where name = 'IX_Vector_Name' and id = object_id('Vector'))
        CREATE INDEX [IX_Vector_Name] ON [Vector]([Name]) ;
    if not exists (select * from sysindexes where name = 'IX_Vector_DisplayOrder_Name' and id = object_id('Vector'))
        CREATE INDEX [IX_Vector_DisplayOrder_Name] ON [Vector]([DisplayOrder],[Name]) ;
    go



    if exists (select * from sysindexes where name = 'UIX_State_Abbreviation') begin
        drop index dbo.State.[UIX_State_Abbreviation] 
    end
    go

    if not exists (select * from sysindexes where name = 'UIX_State_Country_Abbrv') begin
        create unique index [UIX_State_Country_Abbrv] ON [dbo].[State]([CountryID], [Abbreviation])
    end
    go

    if not exists (select * from sysindexes where name = 'IX_CustomerSession_CustomerID' and id = object_id('CustomerSession'))
        CREATE INDEX [IX_CustomerSession_CustomerID] ON [CustomerSession]([CustomerID]) ;


    GO


    if not exists (select * from sysindexes where name = 'IX_ShippingByTotalByPercent_RowGUID' and id = object_id('ShippingByTotalByPercent'))
        CREATE NONCLUSTERED INDEX [IX_ShippingByTotalByPercent_RowGUID] ON [dbo].[ShippingByTotalByPercent] ([RowGUID])
    GO
    if not exists (select * from sysindexes where name = 'IX_ShippingByTotalByPercent_ShippingMethodID_LowValue_HighValue' and id = object_id('ShippingByTotalByPercent'))
        CREATE NONCLUSTERED INDEX [IX_ShippingByTotalByPercent_ShippingMethodID_LowValue_HighValue] ON [dbo].[ShippingByTotalByPercent] ([ShippingMethodID], [LowValue], [HighValue])
    GO

    if not exists (select * from sysindexes where name = 'UIX_ZipTaxRate_ZipCode_TaxClassID' and id = object_id('ZipTaxRate'))
        CREATE UNIQUE NONCLUSTERED INDEX [UIX_ZipTaxRate_ZipCode_TaxClassID] ON [dbo].[ZipTaxRate] ([ZipCode], [TaxClassID])

    GO

    if exists (select * from sysindexes where id = object_id('topic') and name = 'IX_Topic_Name')
        drop index dbo.topic.IX_Topic_Name
    go

    if not exists (select * from sysindexes where id = object_id('topic') and name = 'UIX_Topic_Name')
        create unique index UIX_Topic_Name on dbo.topic (Name)
    go

    if not exists (select * from sysindexes where name = 'UIX_EventhHandler' and id = object_id('EventHandler'))
        CREATE UNIQUE INDEX [UIX_EventhHandler] ON [EventHandler] ([EventName]);
    GO





/****************************************   Views   *******************************************/

    if exists (select * from sysobjects where id = object_id(N'[v_Sales]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_Sales
    go

    if exists (select * from sysobjects where id = object_id(N'[Product_Stats]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view Product_Stats
    go

    if exists (select * from sysobjects where id = object_id(N'[Product_Comments_View]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view Product_Comments_View
    go

    if exists (select * from sysobjects where id = object_id(N'[v_CategoryCounts]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_CategoryCounts
    go

    if exists (select * from sysobjects where id = object_id(N'[v_LIbraryCounts]') and OBJECTPROPERTY(id, N'IsView') = 1)
     drop view v_LibraryCounts
    go


    if exists (select * from sysobjects where id = object_id(N'[v_SectionCounts]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_SectionCounts
    go

    if exists (select * from sysobjects where id = object_id(N'[v_AffiliateCounts]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_AffiliateCounts
    go

    if exists (select * from sysobjects where id = object_id(N'[v_CustomerLevelCounts]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_CustomerLevelCounts
    go

    if exists (select * from sysobjects where id = object_id(N'[v_ManufacturerCounts]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_ManufacturerCounts
    go

    if exists (select * from sysobjects where id = object_id(N'[v_DistributorCounts]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_DistributorCounts
    go

    if exists (select * from sysobjects where id = object_id(N'[vAllIndexes]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view vAllIndexes
    go

    if exists (select * from sysobjects where id = object_id(N'[v_ProductVariantDefaultView]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_ProductVariantDefaultView
    go

    if exists (select * from sysobjects where id = object_id(N'[v_ProductMasterView]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_ProductMasterView
    go

    if exists (select * from sysobjects where id = object_id(N'[v_ProductMasterViewWithOnlyOneVariant]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_ProductMasterViewWithOnlyOneVariant
    go

    if exists (select * from sysobjects where id = object_id(N'[v_LibraryDocumentsMasterView]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_LibraryDocumentsMasterView
    go

    if exists (select * from sysobjects where id = object_id(N'[v_LibraryDocumentsMasterViewWithOnlyOneVariant]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_LibraryDocumentsMasterViewWithOnlyOneVariant
    go

    if exists (select * from sysobjects where id = object_id(N'[v_CategoryProductsMasterView]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_CategoryProductsMasterView
    go

    if exists (select * from sysobjects where id = object_id(N'[v_CategoryProductsMasterViewWithOnlyOneVariant]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_CategoryProductsMasterViewWithOnlyOneVariant
    go

    if exists (select * from sysobjects where id = object_id(N'[v_SectionProductsMasterView]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_SectionProductsMasterView
    go

    if exists (select * from sysobjects where id = object_id(N'[v_SectionProductsMasterViewWithOnlyOneVariant]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_SectionProductsMasterViewWithOnlyOneVariant
    go

    if exists (select * from sysobjects where id = object_id(N'[v_ManufacturerProductsMasterView]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_ManufacturerProductsMasterView
    go

    if exists (select * from sysobjects where id = object_id(N'[v_ManufacturerProductsMasterViewWithOnlyOneVariant]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_ManufacturerProductsMasterViewWithOnlyOneVariant
    go

    if exists (select * from sysobjects where id = object_id(N'[v_DistributorProductsMasterView]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_DistributorProductsMasterView
    go

    if exists (select * from sysobjects where id = object_id(N'[v_DistributorProductsMasterViewWithOnlyOneVariant]') and OBJECTPROPERTY(id, N'IsView') = 1)
        drop view v_DistributorProductsMasterViewWithOnlyOneVariant
    go






    if exists (select * from sysobjects where id = OBJECT_ID(N'[dbo].[ProductEntity]') AND type = 'V') 
        drop view dbo.ProductEntity;
    go

    create view [dbo].[ProductEntity]
    AS
    select 'category' EntityType, ProductID, CategoryID EntityID, DisplayOrder, CreatedOn From dbo.productcategory
    union all
    select 'section', ProductID, SectionID EntityID, DisplayOrder, CreatedOn From dbo.productsection
    union all
    select 'manufacturer', ProductID, ManufacturerID EntityID, DisplayOrder, CreatedOn From dbo.productmanufacturer
    union all
    select 'distributor', ProductID, DistributorID EntityID, DisplayOrder, CreatedOn From dbo.productdistributor
    union all
    select 'affiliate', ProductID, AffiliateID EntityID, DisplayOrder, CreatedOn From dbo.productaffiliate
    union all
    select 'locale', ProductID, LocaleSettingID EntityID, DisplayOrder, CreatedOn From dbo.productlocalesetting
    union all
    select 'customerlevel', ProductID, CustomerLevelID EntityID, DisplayOrder, CreatedOn From dbo.ProductCustomerLevel
    union all
    select 'library', DocumentID, LibraryID EntityID, DisplayOrder, CreatedOn From dbo.DocumentLibrary
    union all
    select 'genre', ProductID, GenreID EntityID, DisplayOrder, CreatedOn From dbo.productgenre
    union all
    select 'vector', ProductID, VectorID EntityID, DisplayOrder, CreatedOn From dbo.productvector
    GO






    if exists (select * from sysobjects where id = OBJECT_ID(N'[dbo].[EntityMaster]') AND type = 'V') 
        drop view EntityMaster;
    go
    create view dbo.EntityMaster
    AS

        SELECT 'category' EntityType, Entity.CategoryID EntityID, Entity.CategoryGUID EntityGuid, Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentCategoryID ParentEntityID,DisplayOrder,SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor,GraphicsColor,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
        FROM dbo.Category Entity with (NOLOCK)
          left join (SELECT pc.CategoryID, COUNT(pc.ProductID) AS NumProducts
                     FROM  dbo.ProductCategory pc with (nolock)
                         join [dbo].Product p with (nolock) on pc.ProductID = p.ProductID  and p.deleted=0 and p.published=1
                     GROUP BY pc.CategoryID
                    ) a on Entity.CategoryID = a.CategoryID
        WHERE Published = 1 and Deleted=0

        UNION ALL

        SELECT 'affiliate' EntityType, Entity.AffiliateID EntityID,Entity.AffiliateGUID EntityGuid, Name,4 as ColWidth,'' as Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentAffiliateID ParentEntityID,DisplayOrder,0 as SortByLooks,'' as XmlPackage,Published,'' as ContentsBGColor,'' as PageBGColor,'' as GraphicsColor,isnull(NumProducts, 0) NumObjects, 0 PageSize, 0 QuantityDiscountID, '' Summary, SkinID, TemplateName
        FROM dbo.Affiliate Entity with (NOLOCK)
          left join (SELECT pa.AffiliateID, COUNT(pa.ProductID) AS NumProducts
                     FROM dbo.ProductAffiliate pa with (nolock) join [dbo].Product p with (nolock) on pa.ProductID = p.ProductID and p.deleted=0 and p.published=1
                     GROUP BY pa.AffiliateID
                    ) a on Entity.AffiliateID = a.AffiliateID
        WHERE Published = 1 and Deleted=0

        UNION ALL

        SELECT 'section' EntityType, Entity.SectionID EntityID,Entity.SectionGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentSectionID ParentEntityID,DisplayOrder,SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor,GraphicsColor,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
        FROM dbo.Section Entity with (NOLOCK)
            left join (SELECT ps.SectionID, COUNT(ps.ProductID) AS NumProducts
                       FROM dbo.ProductSection ps with (nolock) join [dbo].Product p with (nolock) on ps.ProductID = p.ProductID and p.deleted=0 and p.published=1
                       GROUP BY ps.SectionID
                      ) a on Entity.SectionID = a.SectionID
        WHERE Published = 1 and Deleted=0

        UNION ALL

        SELECT 'manufacturer' EntityType, Entity.ManufacturerID EntityID,Entity.ManufacturerGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentManufacturerID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor,GraphicsColor,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
        FROM dbo.Manufacturer Entity with (NOLOCK)
        left join (SELECT pm.ManufacturerID, COUNT(pm.ProductID) AS NumProducts
                   FROM dbo.ProductManufacturer pm with (nolock) join [dbo].Product p with (nolock) on pm.ProductID = p.ProductID and p.deleted=0 and p.published=1
                   GROUP BY pm.ManufacturerID
                  ) a on Entity.ManufacturerID = a.ManufacturerID
        WHERE Published = 1 and Deleted=0

        UNION ALL

        SELECT 'library' EntityType, Entity.LibraryID EntityID,Entity.LibraryGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentLibraryID ParentEntityID,DisplayOrder,SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor,GraphicsColor,isnull(NumDocuments, 0) NumObjects, PageSize, 0 QuantityDiscountID, Summary, SkinID, TemplateName
        FROM dbo.Library Entity with (NOLOCK)
            left join (SELECT dl.LibraryID, COUNT(dl.DocumentID) AS NumDocuments
                       FROM  dbo.DocumentLibrary dl with (nolock) 
                           join [dbo].[Document] d with (nolock) on d.DocumentID = dl.DocumentID and d.deleted=0 and d.published=1
                       GROUP BY dl.LibraryID
                      ) a on Entity.LibraryID = a.LibraryID
        WHERE Published = 1 and Deleted=0

        UNION ALL

        SELECT 'distributor' EntityType, Entity.DistributorID EntityID,Entity.DistributorGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentDistributorID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor,GraphicsColor,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
        FROM dbo.Distributor Entity with (NOLOCK)
            left join (SELECT pd.DistributorID, COUNT(pd.ProductID) AS NumProducts
                       FROM dbo.ProductDistributor pd with (nolock) join [dbo].Product p with (nolock) on pd.ProductID = p.ProductID and p.deleted=0 and p.published=1
                       GROUP BY pd.DistributorID
                      ) a on Entity.DistributorID = a.DistributorID
        WHERE Published = 1 and Deleted=0

        UNION ALL

        SELECT 'genre' EntityType, Entity.GenreID EntityID,Entity.GenreGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentGenreID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor,GraphicsColor,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
        FROM dbo.Genre Entity with (NOLOCK)
            left join (SELECT px.GenreID, COUNT(px.ProductID) AS NumProducts
                       FROM dbo.ProductGenre px with (nolock) join [dbo].Product p with (nolock) on px.ProductID = p.ProductID and p.deleted=0 and p.published=1
                       GROUP BY px.GenreID
                      ) a on Entity.GenreID = a.GenreID
        WHERE Published = 1 and Deleted=0

        UNION ALL

        SELECT 'vector' EntityType, Entity.VectorID EntityID,Entity.VectorGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentVectorID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor,GraphicsColor,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
        FROM dbo.Vector Entity with (NOLOCK)
            left join (SELECT px2.VectorID, COUNT(px2.ProductID) AS NumProducts
                       FROM dbo.ProductVector px2 with (nolock) join [dbo].Product p with (nolock) on px2.ProductID = p.ProductID and p.deleted=0 and p.published=1
                       GROUP BY px2.VectorID
                      ) a on Entity.VectorID = a.VectorID
        WHERE Published = 1 and Deleted=0

        UNION ALL

        SELECT 'customerLevel' EntityType, Entity.CustomerLevelID EntityID,Entity.CustomerLevelGUID EntityGuid,Name, 4 ColWidth, '' Description,SEName, '' SEKeywords, '' SEDescription, '' SETitle, '' SENoScript,'' SEAltText,ParentCustomerLevelID ParentEntityID,DisplayOrder,0 SortByLooks, '' XmlPackage, 1 Published,'' ContentsBGColor, '' PageBGColor, '' GraphicsColor,isnull(NumProducts, 0) NumObjects, 20 PageSize, 0 QuantityDiscountID, '' Summary, SkinID, TemplateName
        FROM dbo.CustomerLevel Entity with (NOLOCK)
          left join (SELECT pc.CustomerLevelID, COUNT(pc.ProductID) AS NumProducts
                     FROM  dbo.ProductCustomerLevel pc with (nolock)
                         join [dbo].Product p with (nolock) on pc.ProductID = p.ProductID  and p.deleted=0 and p.published=1
                     GROUP BY pc.CustomerLevelID
                    ) a on Entity.CustomerLevelID = a.CustomerLevelID
        WHERE Deleted=0

    GO



/***************************************   Functions   ****************************************/

    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[GetQtyDiscountPct]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
        DROP FUNCTION [dbo].[GetQtyDiscountPct]
    GO

    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[GetQtyDiscountAmount]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
        DROP FUNCTION [dbo].[GetQtyDiscountAmount]
    GO








    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[FindQtyDiscountID]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
        DROP FUNCTION dbo.FindQtyDiscountID
    GO
    CREATE FUNCTION dbo.FindQtyDiscountID(@entityid int, @entitytype varchar(20))
    RETURNS int
    AS
    BEGIN
        DECLARE @qid int, @pentid int
        SELECT @qid = QuantityDiscountID, @pentid = ParentEntityID from dbo.entitymaster where entityid = @entityid and EntityType = @entitytype

        IF (isnull(@qid, 0) = 0) and (isnull(@pentid, 0) <> 0) BEGIN
            select @qid = dbo.FindQtyDiscountID(@pentid, @entitytype)
        END

        RETURN isnull(@qid, 0)
    END

    GO






    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[GetQtyDiscountID]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
        DROP FUNCTION dbo.GetQtyDiscountID
    GO
    CREATE FUNCTION [dbo].[GetQtyDiscountID](@productid int)
    RETURNS int
    AS
    BEGIN

        DECLARE @did int
        SET @did = 0

        SELECT @did =  isnull(QuantityDiscountID, 0) FROM dbo.product with (nolock) WHERE productid = @productid

        IF @did = 0
            select top 1 @did = dbo.FindQtyDiscountID(pe.entityid, pe.EntityType) 
            From dbo.productentity pe join dbo.EntityMaster em on pe.EntityType = em.EntityType and pe.entityid = em.entityid 
            where pe.EntityType in ('category', 'section', 'manufacturer') and productid = @productid and dbo.FindQtyDiscountID(pe.entityid, pe.EntityType) > 0
            order by case pe.EntityType when 'category' then 1 when 'section' then 2 when 'manufacturer' then 3 end, em.parententityid, pe.displayorder


        IF @did = 0 BEGIN
            declare @cfg nvarchar(100)
            select @cfg = configvalue from dbo.AppConfig with (nolock) where [name] = 'FromActiveQuantityDiscountTable'
            select @did = QuantityDiscountID from dbo.QuantityDiscount with (nolock) where [name] like '%' + @cfg + '%'
        END 

        RETURN @did
    END 

    GO






    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[GetQtyDiscount]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
        DROP FUNCTION dbo.GetQtyDiscount
    GO
    CREATE FUNCTION [dbo].[GetQtyDiscount](@productid int, @Qty int, @discounttype tinyint)
    RETURNS decimal(15,6)
    AS
    BEGIN

        DECLARE @did int, @qtydiscount decimal(15,6)
        SET @did = 0
        SET @qtydiscount = 0

        SELECT @did =  dbo.GetQtyDiscountID(@productid)

        IF @did = 0 
            SET @qtydiscount = 0
        ELSE BEGIN
            SELECT @qtydiscount = qdt.DiscountPercent FROM dbo.QuantityDiscountTable qdt with (nolock) join dbo.QuantityDiscount qd with (nolock) on qdt.QuantityDiscountID = qd.QuantityDiscountID WHERE qd.quantitydiscountid = @did and qdt.LowQuantity <= @Qty and qdt.HighQuantity >= @Qty and qd.DiscountType = @discounttype
        END 

        RETURN @qtydiscount
    END 

    GO








    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[ZeroFloor]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
        DROP FUNCTION dbo.ZeroFloor
    GO
    CREATE FUNCTION dbo.ZeroFloor(@value decimal(15, 6))
    RETURNS decimal(15, 6)
    AS BEGIN
        IF @value < 0
            SET @value = 0
        RETURN @value 
    END

    GO







    IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[Split]') AND xtype in (N'FN', N'IF', N'TF'))
        DROP FUNCTION dbo.Split
    GO
    CREATE FUNCTION dbo.Split(@String ntext, @Delimiter char(1))
    RETURNS @Results TABLE (Items nvarchar(4000))
    AS
    BEGIN
        DECLARE @STARTINDEX INT, @ENDINDEX INT, @STRINGLEN int, @LOOPSTRING NVARCHAR(4000)
        SET @STRINGLEN = DATALENGTH(ISNULL(@STRING, N''))/2
        SET @STARTINDEX = 1

        DECLARE @SLICE nvarchar(4000)
        DECLARE @SUBSTR nvarchar(4000)

        SET @SLICE  = N''

        WHILE @STARTINDEX < @STRINGLEN+1 BEGIN
            
            SET @SUBSTR = SUBSTRING(@STRING, @STARTINDEX, 1)

            IF @SUBSTR = @Delimiter BEGIN
                IF @SLICE != N''
                    INSERT INTO @Results(Items) VALUES(@SLICE)
                SET @SLICE = N''
            END
            ELSE
                SET @SLICE = @SLICE+@SUBSTR

            SET @STARTINDEX = @STARTINDEX + 1
        END
        IF LEN(@SLICE) > 0
            INSERT INTO @Results(Items) VALUES(@SLICE)
        RETURN
    END
    GO







    IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[MakeSEName]') AND xtype in (N'FN', N'IF', N'TF'))
        DROP FUNCTION dbo.MakeSEName
    GO
    create function dbo.MakeSEName(@string varchar(8000))
    RETURNS varchar(4000)
    AS
    BEGIN
        declare @charindex int, @newstring varchar(8000)

        set @string = replace(replace(replace(@string, ' ', '-'), '---', '-'), '--', '-')
            
        set @newstring = ''

        select @charindex = PATINDEX('%[^a-z0-9_-]%', @string)

        IF @charindex = len(@string)
            select @newstring = left(@string, @charindex-1)
            
        ELSE BEGIN
            select @newstring = @newstring + left(@string, @charindex-1), @string = substring(@string, @charindex+1, len(@string)-@charindex+1)
            WHILE PATINDEX('%[^a-z0-9_-]%', @string) > 0 BEGIN
                select @charindex = PATINDEX('%[^a-z0-9_-]%', @string)
                IF @charindex = len(@string)
                    select @newstring = @newstring + left(@string, @charindex-1), @string = ''
                ELSE
                    select @newstring = @newstring + left(@string, @charindex-1), @string = substring(@string, @charindex+1, len(@string)-@charindex+1)
            END
        END
        RETURN lower(@newstring)

    END
    go


    IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[GetIndexColumnOrder]') AND xtype in (N'FN', N'IF', N'TF'))
        DROP FUNCTION dbo.GetIndexColumnOrder
    GO
    CREATE FUNCTION [dbo].[GetIndexColumnOrder] 
    ( 
        @object_id INT, 
        @index_id TINYINT, 
        @column_id TINYINT 
    ) 
    RETURNS NVARCHAR(5) 
    AS 
    BEGIN 
        DECLARE @r NVARCHAR(5) 
        SELECT @r = CASE INDEXKEY_PROPERTY 
        ( 
            @object_id, 
            @index_id, 
            @column_id, 
            'IsDescending' 
        ) 
            WHEN 1 THEN N' DESC' 
            ELSE N'' 
        END 
        RETURN @r 
    END 

    GO


    IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[getSectionPath]') AND xtype in (N'FN', N'IF', N'TF'))
        DROP FUNCTION dbo.getSectionPath
    GO
    create function [dbo].[getSectionPath](@sectionID int)
        RETURNS nvarchar(4000)
    AS
    BEGIN
        DECLARE @Sectionpath nvarchar(4000), @SecID int, @SectionName nvarchar(400)
        SET @Sectionpath  = ''
        
        SELECT @SecID = ParentSectionID, @SectionName = Name From dbo.Section with (nolock) where SectionID = @sectionID 
        WHILE @@rowcount > 0 BEGIN
            SET @Sectionpath = '\' + @SectionName + @Sectionpath 
            SELECT @SecID = ParentSectionID, @SectionName = Name From dbo.Section with (nolock) where SectionID = @SecID 
        END
        
        RETURN @Sectionpath 
    END


    GO


    -- ---------------------------------------------------------------------------
    -- dbo.GetIndexColumns
    -- Returns the list of columns in the index 
    -- ---------------------------------------------------------------------------
    IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[GetIndexColumns]') AND xtype in (N'FN', N'IF', N'TF'))
        DROP FUNCTION dbo.GetIndexColumns
    GO
    CREATE FUNCTION [dbo].[GetIndexColumns] 
    ( 
        @table_name SYSNAME, 
        @object_id INT, 
        @index_id TINYINT 
    ) 
    RETURNS NVARCHAR(4000) 
    AS 
    BEGIN 
        DECLARE 
            @colnames NVARCHAR(4000),  
            @thisColID INT, 
            @thisColName SYSNAME 
             
        SET @colnames = INDEX_COL(@table_name, @index_id, 1) 
            + dbo.GetIndexColumnOrder(@object_id, @index_id, 1) 
     
        SET @thisColID = 2 
        SET @thisColName = INDEX_COL(@table_name, @index_id, @thisColID) 
            + dbo.GetIndexColumnOrder(@object_id, @index_id, @thisColID) 
     
        WHILE (@thisColName IS NOT NULL) 
        BEGIN 
            SET @thisColID = @thisColID + 1 
            SET @colnames = @colnames + ', ' + @thisColName 
     
            SET @thisColName = INDEX_COL(@table_name, @index_id, @thisColID) 
                + dbo.GetIndexColumnOrder(@object_id, @index_id, @thisColID) 
        END 
        RETURN @colNames 
    END 

    GO







    IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[getCategoryPath]') AND xtype in (N'FN', N'IF', N'TF'))
        DROP FUNCTION dbo.getCategoryPath
    GO
    CREATE function [dbo].[getCategoryPath](@categoryID int)
        RETURNS nvarchar(4000)
    AS
    BEGIN
        DECLARE @categorypath nvarchar(4000), @catID int, @catName nvarchar(400)
        SET @categorypath  = ''
        
        SELECT @catID = ParentCategoryID, @catName = Name From dbo.category with (nolock) where CategoryID = @categoryID 
        WHILE @@rowcount > 0 BEGIN
            SET @categorypath = '\' + @catName + @categorypath 
            SELECT @catID = ParentCategoryID, @catName = Name From dbo.category with (nolock) where CategoryID = @catID 
        END
        
        RETURN @categorypath 
    END





    GO







    IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[GetCustomerLevelPrice]') AND xtype in (N'FN', N'IF', N'TF'))
        DROP FUNCTION dbo.GetCustomerLevelPrice
    go
    CREATE FUNCTION dbo.GetCustomerLevelPrice(@VariantID int, @CustomerLevelID int)
    RETURNS decimal(15, 6)
    AS
    BEGIN
        DECLARE @levelprice money, @LevelDiscountPercent money, @LevelDiscountsApplyToExtendedPrices tinyint
        SELECT @LevelDiscountPercent = LevelDiscountPercent, @LevelDiscountsApplyToExtendedPrices = LevelDiscountsApplyToExtendedPrices FROM dbo.customerlevel with (nolock) WHERE customerlevelid = @CustomerLevelID    

        SELECT @levelprice = case 
                              when @CustomerLevelID = 0 and pv.SalePrice is not null and pv.SalePrice > 0 then pv.SalePrice
                              when e.price is not null and pcl.CustomerLevelID is not null then case when isnull(@LevelDiscountsApplyToExtendedPrices, 0) = 1 and isnull(@LevelDiscountPercent, 0.0) > 0 then (e.price * (100.0-@LevelDiscountPercent))/100.0 else e.price end
                              else case when isnull(@LevelDiscountPercent, 0.0) > 0 then round((pv.Price * (100.0-@LevelDiscountPercent))/100.0, 2) else pv.Price end
                             end
        FROM dbo.ProductVariant pv with (nolock) 
            left join dbo.ExtendedPrice e with (nolock) on pv.VariantID = e.VariantID and e.CustomerLevelID = @CustomerLevelID
            left join ProductCustomerlevel pcl with (nolock) on pv.ProductID = pcl.ProductID and pcl.CustomerLevelID  =@CustomerLevelID
        WHERE pv.VariantID = @VariantID


        RETURN @levelprice
    END



    GO









    IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[KitPriceDelta]') AND xtype in (N'FN', N'IF', N'TF'))
        DROP FUNCTION dbo.KitPriceDelta
    go
    CREATE FUNCTION dbo.KitPriceDelta(@CustomerID int, @ProductID int, @ShoppingCartRecID int)
    RETURNS decimal(15, 6)
    AS
    BEGIN
        DECLARE @deltaprice money

        SELECT @deltaprice = sum(quantity*pricedelta) 
        FROM dbo.kitcart kc with (NOLOCK)  join dbo.kititem ki with (NOLOCK) on kc.kititemid = ki.kititemid 
        WHERE CustomerID = @CustomerID and ProductID = @ProductID and ShoppingCartRecid = @ShoppingCartRecID

        RETURN @deltaprice
    END

    GO








    IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[KitWeightDelta]') AND xtype in (N'FN', N'IF', N'TF'))
        DROP FUNCTION dbo.KitWeightDelta
    go
    CREATE FUNCTION dbo.KitWeightDelta(@CustomerID int, @ProductID int, @ShoppingCartRecID int)
    RETURNS money
    AS
    BEGIN
        DECLARE @deltaweight money

        SELECT @deltaweight = sum(quantity*weightdelta) 
        FROM dbo.kitcart kc with (NOLOCK)  join dbo.kititem ki with (NOLOCK) on kc.kititemid = ki.kititemid 
        WHERE CustomerID = @CustomerID and ProductID = @ProductID and ShoppingCartRecid = @ShoppingCartRecID

        RETURN @deltaweight

    END

    GO









    IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[PackPriceDelta]') AND xtype in (N'FN', N'IF', N'TF'))
        DROP FUNCTION dbo.PackPriceDelta
    go
    CREATE FUNCTION dbo.PackPriceDelta(@CustomerID int, @CustomerLevelID int, @PackID int, @ShoppingCartRecID int)
    RETURNS decimal(15, 6)
    AS
    BEGIN
        DECLARE @deltaprice money

        select @deltaprice = sum(dbo.GetCustomerLevelPrice(VariantID, @CustomerLevelID)*Quantity)
        FROM dbo.customcart with (nolock) 
        WHERE CustomerID = @CustomerID and PackID = @PackID and ShoppingCartRecid = @ShoppingCartRecID

        RETURN @deltaprice
    END

    GO




    IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[ExchangeRate]') AND xtype in (N'FN', N'IF', N'TF'))
        DROP FUNCTION dbo.ExchangeRate
    go
    create function dbo.ExchangeRate(@CurrencyCode varchar(3))
    RETURNS money
    AS
    BEGIN
        declare @SourceExchangerate money, @TargetExchangerate money, @Exchangerate money, @StoreBaseCurrency varchar(3)
        select @StoreBaseCurrency = ConfigValue from dbo.Appconfig with (nolock) where [name] = 'Localization.StoreCurrency'
        select @TargetExchangerate = ExchangeRate from dbo.currency with (nolock) where CurrencyCode = @CurrencyCode
        select @SourceExchangerate = ExchangeRate from dbo.currency with (nolock) where CurrencyCode = @StoreBaseCurrency
        if @SourceExchangerate is null or @SourceExchangerate = 0 or @TargetExchangerate is null or @TargetExchangerate = 0
            set @exchangerate = 1
        else
            set @exchangerate = @TargetExchangerate/@SourceExchangerate

        RETURN @exchangerate
    END


    GO








/*************************************   Stored Procs   ***************************************/
IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_GetNews]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[aspdnsf_GetNews]
GO

CREATE PROCEDURE [dbo].[aspdnsf_GetNews] (
	@NumHomeNewsToDisplay INT
)
AS
BEGIN
	SET NOCOUNT ON

	select TOP(@NumHomeNewsToDisplay) * 
	from News  with (NOLOCK)  
	where ExpiresOn > getdate() and Deleted = 0 and Published = 1 
	order by NewsId desc 
END
GO

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_GetFeaturedProducts]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[aspdnsf_GetFeaturedProducts]
GO

CREATE PROCEDURE [dbo].[aspdnsf_GetFeaturedProducts](
	@FeaturedCategoryID INT,
	@NumHomePageFeaturedProducts INT,
	@CustomerLevelID INT
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
	where pc.CategoryID=@FeaturedCategoryID and p.Deleted=0
	order by newid()

END
GO

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_GetRecentlyViewedProducts]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[aspdnsf_GetRecentlyViewedProducts]
GO
CREATE PROCEDURE [dbo].[aspdnsf_GetRecentlyViewedProducts]
	@productID		int, 
	@CustomerViewID	varchar(50),
	@invFilter		int,
	@recentlyViewedProductsNumToDisplay int	
WITH ENCRYPTION     
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
		 LEFT JOIN (SELECT variantid, SUM(quan) inventory FROM inventory GROUP BY variantid) i ON pvt.variantid = i.variantid 	
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



IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_GetCustomersRelatedProducts]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[aspdnsf_GetCustomersRelatedProducts]
GO
CREATE PROCEDURE [dbo].[aspdnsf_GetCustomersRelatedProducts]
	@CustomerViewID nvarchar(50),
	@ProductID int,
	@CustomerLevelID int,
	@InvFilter int,
	@affiliateID int
WITH ENCRYPTION 
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


IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_insProductView]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[aspdnsf_insProductView]
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

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_SynchronizeCart]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[aspdnsf_SynchronizeCart]
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
	FROM dbo.ShoppingCart sc
		INNER JOIN  dbo.Product p with (NOLOCK)  ON p.ProductID = sc.ProductID AND sc.CustomerID = @CustomerID
		JOIN dbo.ProductVariant pv with (NOLOCK) on sc.VariantID = pv.VariantID 
		LEFT JOIN dbo.Inventory i with (NOLOCK) on pv.VariantID = i.VariantID AND i.size = sc.ChosenSize AND i.color = sc.ChosenColor		
    WHERE sc.CustomerID = @CustomerID and
          sc.CartType  = @CartType
END
GO




IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_getKitItems]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[aspdnsf_getKitItems]
GO


CREATE PROCEDURE [dbo].[aspdnsf_getKitItems]
	@ProductID int,
    @CartRecID int,
	@CustomerID int
WITH ENCRYPTION  	
AS
BEGIN
	SET NOCOUNT ON	
	SELECT
			ki.KitItemID,
			kg.KitGroupID,
			kg.ProductID,
			ki.[Name]        AS ItemName,
			ki.[Description] AS ItemDescription,
			ki.PriceDelta    AS ItemPriceDelta,
			ki.WeightDelta   AS ItemWeightDelta, 
			ki.IsDefault,		
			CAST( ( CASE        
			WHEN   kc.KitItemID IS NOT NULL THEN 1       
			WHEN ( kc.KitItemID IS NULL AND ki.IsDefault = 1 ) THEN 1
			ELSE 0       
			END ) AS BIT )   AS IsSelected,
			CASE	WHEN   kc.KitItemID IS NOT NULL THEN kc.TextOption       			
					ELSE ''
			END AS TextOption,
			kg.[Name]		 AS GroupName,
			kg.[Description] AS GroupDescription,
			kg.DisplayOrder	 AS GroupDisplayOrder,
			kg.IsRequired,
			kgt.KitGroupTypeID AS SelectionControl
    FROM KitItem      ki  with (nolock) 
    JOIN KitGroup     kg  with (nolock) ON kg.KitGroupID=ki.KitGroupID 
    JOIN KitGroupType kgt with (nolock) ON kgt.KitGroupTypeID=kg.KitGroupTypeID
    LEFT JOIN KitCart kc  with (nolock) ON ( kc.KitItemID = ki.KitItemID AND kc.KitGroupID = ki.KitGroupID 
                                                  AND kc.ProductID = kg.ProductID  
                                                  AND kc.CustomerID = @CustomerID 
                                                  AND kc.ShoppingCartrecID = @CartRecID AND kc.ShoppingCartrecID <> 0)  
    WHERE	kg.ProductID=@ProductID 	
    ORDER BY kg.DisplayOrder ASC, ki.DisplayOrder ASC, ki.[Name]

END

GO






    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_updOrderItemQuantityDiscount]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_updOrderItemQuantityDiscount]
    GO
    CREATE PROC [dbo].[aspdnsf_updOrderItemQuantityDiscount]
        @OrderNumber int 
    WITH ENCRYPTION         
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




    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_UpdateCartItemQuantity]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_UpdateCartItemQuantity]
    go

    CREATE PROCEDURE [dbo].[aspdnsf_UpdateCartItemQuantity]
        @ProductID int,
        @VariantID int,
        @ShoppingCartRecID INT,
        @Quantity INT,
        @NewQuantity INT OUTPUT
    WITH ENCRYPTION      
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

    go






    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_AddItemToCart]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[aspdnsf_AddItemToCart]
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
    @NewShoppingCartRecID int OUTPUT
WITH ENCRYPTION      
AS
SET NOCOUNT ON


DECLARE @RestrictedQy varchar(8000), @CurrentCartQty int, @InventoryFullSKU nvarchar(50), @InventoryWeightDelta money, @AllowEmptySkuAddToCart varchar(10), @ShoppingCartrecid int, @IsAKit tinyint, @IsAPack tinyint
DECLARE @levelprice money, @LevelDiscountPercent money, @LevelDiscountsApplyToExtendedPrices tinyint

SELECT @RestrictedQy = RestrictedQuantities FROM dbo.ProductVariant with (nolock) WHERE VariantID = @VariantID

SELECT	@IsAKit = IsAKit, @IsAPack = IsAPack FROM dbo.Product with (nolock) WHERE ProductID = @ProductID 


SELECT @CurrentCartQty = Quantity From dbo.shoppingcart with (nolock) where ShippingAddressID = @ShippingAddressID and ProductID = @ProductID and VariantID = @VariantID and ChosenColor = @ChosenColor and ChosenSize = @ChosenSize and TextOption like @TextOption and CustomerID = @CustomerID and GiftRegistryForCustomerID = @GiftRegistryForCustomerID and CartType = @CartType

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
IF @CurrentCartQty is not null and @IsAKit = 0 and @IsAPack = 0 BEGIN
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
INSERT dbo.ShoppingCart(CartType,ShoppingCartRecGUID,CustomerID,ShippingAddressID,BillingAddressID,ProductID,SubscriptionInterval,SubscriptionIntervalType,VariantID,ProductSKU,ProductPrice,CustomerEntersPrice,ProductWeight,ProductDimensions,Quantity,RequiresCount,ChosenColor,ChosenColorSKUModifier,ChosenSize,ChosenSizeSKUModifier,TextOption,IsTaxable,IsShipSeparately,IsDownload,DownloadLocation,FreeShipping,DistributorID,RecurringInterval,RecurringIntervalType, IsSystem, IsAKit, IsAPack, TaxClassID, IsKit2)
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
	@IsKit2
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

















    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_WSIUpdateMappings]'))
        DROP PROCEDURE [dbo].[aspdnsf_WSIUpdateMappings]
    GO
    create proc dbo.aspdnsf_WSIUpdateMappings
        @xml text
    WITH ENCRYPTION     
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




    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_CheckFilthy]'))
        DROP PROCEDURE [dbo].[aspdnsf_CheckFilthy]
    GO
    create proc dbo.aspdnsf_CheckFilthy
        @commenttext ntext,
        @locale char(5)  
        WITH ENCRYPTION 
    AS
    SET NOCOUNT ON

        IF exists (select * from dbo.Split(@commenttext, ' ') c join dbo.BadWord b with (nolock) on c.items = b.Word and b.LocaleSetting = @locale)
            SELECT 1 IsFilthy
        ELSE
            SELECT 0 IsFilthy


    GO





    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_EditOrder]'))
        DROP PROCEDURE [dbo].[aspdnsf_EditOrder]
    GO
    create proc dbo.aspdnsf_EditOrder
        @OrderNumber int
        WITH ENCRYPTION 
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







    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_getBadWord]'))
        DROP PROCEDURE [dbo].[aspdnsf_getBadWord]
    GO
    create proc [dbo].[aspdnsf_getBadWord]
        @BadWordID int   = null
        WITH ENCRYPTION 
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

    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_insBadWord]'))
        DROP PROCEDURE [dbo].[aspdnsf_insBadWord]
    GO
    create proc [dbo].[aspdnsf_insBadWord]
        @LocaleSetting nvarchar(10),
        @Word nvarchar(100),
        @BadWordID int OUTPUT
        WITH ENCRYPTION 
    AS
    SET NOCOUNT ON 

    INSERT INTO [dbo].[BadWord] (LocaleSetting, Word, CreatedOn)
    VALUES(@LocaleSetting,@Word,getdate())

    set @BadWordId = @@Identity


    GO

    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_updBadWord]'))
        DROP PROCEDURE [dbo].[aspdnsf_updBadWord]
    GO
    create proc [dbo].[aspdnsf_updBadWord]
        @BadWordID int,
        @LocaleSetting nvarchar(10)      = null,
        @Word nvarchar(100)              = null
        WITH ENCRYPTION 
    AS    
    UPDATE [dbo].[BadWord]
       SET [LocaleSetting]               = COALESCE(@LocaleSetting,[LocaleSetting]),
           [Word]                        = COALESCE(@Word,[Word])
     WHERE BadWordID = @BadWordID

    GO




    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_getEventHandler]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_getEventHandler]
    GO
    create proc [dbo].[aspdnsf_getEventHandler]
        @EventID int = null
        WITH ENCRYPTION 
    AS
    SET NOCOUNT ON 

        SELECT EventID, [EventName], CalloutURL, XMLPackage, Active, Debug
        FROM dbo.EventHandler with (nolock) 
        WHERE EventID = COALESCE(@EventID, EventID)
        ORDER BY [EventName]
    GO


    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_updEventHandler]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_updEventHandler]
    GO
    create proc [dbo].[aspdnsf_updEventHandler]
        @EventID int,
        @EventName nvarchar(20) = null,
        @CalloutURL varchar(200) = null,
        @XmlPackage varchar(100) = null,
        @Active bit = null,
        @Debug bit =null
        WITH ENCRYPTION 
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




    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_insEventHandler]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_insEventHandler]
    GO
    create proc [dbo].[aspdnsf_insEventHandler]
        @EventName nvarchar(20),
        @CalloutURL varchar(200),
        @XmlPackage varchar(100),
        @Active bit,
        @Debug bit,
        @EventID int OUTPUT
        WITH ENCRYPTION 
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






    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_updAppconfig]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_updAppconfig]
    GO
    create proc dbo.aspdnsf_updAppconfig
        @AppConfigID int,
        @Description ntext = null,
        @ConfigValue nvarchar(1000) = null,
        @GroupName nvarchar(100) = null,
        @SuperOnly tinyint = null
        WITH ENCRYPTION 
    AS
    SET NOCOUNT ON 


        UPDATE dbo.Appconfig
        SET 
            Description = COALESCE(@Description, Description),
            ConfigValue = COALESCE(@ConfigValue, ConfigValue),
            GroupName = COALESCE(@GroupName, GroupName),
            SuperOnly = COALESCE(@SuperOnly, SuperOnly)
        WHERE AppConfigID = @AppConfigID



    GO

    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_updAffiliate]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_updAffiliate]
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
        WITH ENCRYPTION
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






    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_insProductType]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_insProductType]
    GO
    create proc [dbo].[aspdnsf_insProductType]
        @Name nvarchar(400),
        @ProductTypeID int OUTPUT
        WITH ENCRYPTION
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
     
     
     

    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_getOrder]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_getOrder]
    GO
    create proc dbo.aspdnsf_getOrder
        @ordernumber int
        WITH ENCRYPTION
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











    go


    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_updOrders]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_updOrders]
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
        WITH ENCRYPTION
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










    go

    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_getStringresource]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_getStringresource]
    GO

    create proc [dbo].[aspdnsf_getStringresource]
        @StringResourceID int = null
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    SELECT StringResourceID, StringResourceGUID, Name, LocaleSetting, ConfigValue, CreatedOn, Modified
    FROM dbo.Stringresource with (nolock) 
    WHERE StringResourceID = COALESCE(@StringResourceID, StringResourceID)


    GO







    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_insStringresource]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_insStringresource]
    GO
    create proc [dbo].[aspdnsf_insStringresource]
        @Name nvarchar(100),
        @LocaleSetting nvarchar(10),
        @ConfigValue nvarchar(2500),
        @StringResourceID int OUTPUT
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 

    if exists (select * FROM dbo.Stringresource with (nolock) where [Name] = @Name and LocaleSetting = @LocaleSetting) begin
        set @StringResourceID = -1
    end
    else begin
        insert dbo.Stringresource(StringResourceGUID, Name, LocaleSetting, ConfigValue, CreatedOn, Modified)
        values (newid(), @Name, @LocaleSetting, @ConfigValue, getdate(), 0)

        set @StringResourceID = @@identity
    end 


    GO






    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_updStringresource]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_updStringresource]
    GO
    create proc [dbo].[aspdnsf_updStringresource]
        @StringResourceID int,
        @Name nvarchar(100),
        @LocaleSetting nvarchar(10),
        @ConfigValue nvarchar(2500)
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    UPDATE dbo.Stringresource
    SET 
        Name = COALESCE(@Name, Name),
        LocaleSetting = COALESCE(@LocaleSetting, LocaleSetting),
        ConfigValue = COALESCE(@ConfigValue, ConfigValue),
        Modified = 1
    WHERE StringResourceID = @StringResourceID




    GO








    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_MoveToShoppingCart]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_MoveToShoppingCart]
    GO
    create proc [dbo].[aspdnsf_MoveToShoppingCart]
        @ShoppingCartRecId int,
        @CartType int
        WITH ENCRYPTION
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




    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_GetCartSubTotalAndTax]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_GetCartSubTotalAndTax]
    go
    CREATE proc [dbo].[aspdnsf_GetCartSubTotalAndTax]
        @customerid                       int,
        @customerlevel                    int,
        @includediscounts                 tinyint,
        @couponcode                       nvarchar(50),
        @includeSystemItems               tinyint,
        @IncludeOnlyTaxable               tinyint,
        @includeDownloadItems             tinyint,
        @includeFreeShippingItems         tinyint,
        @IncludeTaxInSubTotal             tinyint = 0,
        @CartType                         tinyint = 0, -- ShoppingCart = 0, WishCart = 1, RecurringCart = 2, GiftRegistryCart = 3
        @OriginalRecurringOrderNumber     int = 0, 
        @OnlyLoadRecurringItemsThatAreDue tinyint = 0, 
        @CurrencyCode                     nvarchar(10),
        @AddressID                        int = 0,
        @debug                            tinyint = 0,
        @Country                          nvarchar(50) = null,  -- this is use by estimator
        @State                            nvarchar(50) = null,  -- this is use by estimator
        @PostalCode                       nvarchar(50) = null   -- this is use by estimator
    WITH ENCRYPTION 
    AS
    SET NOCOUNT ON 

    --get exchange rate
    declare @Exchangerate money
    set @exchangerate =  dbo.ExchangeRate(@CurrencyCode)

    --get order options
    declare @orderoptions nvarchar(4000), @custtaxingaddressid int, @taxcalcmode varchar(20), @DefaultShippingAddressID int
    select @taxcalcmode = configvalue from dbo.appconfig with (nolock) where [name] = 'TaxCalcMode'
    select @orderoptions = OrderOptions, @custtaxingaddressid = case @taxcalcmode when 'billing' then BillingAddressID else ShippingAddressID end, @DefaultShippingAddressID = ShippingAddressID from dbo.customer with (nolock) where customerid = @customerid

    --get customer level discount settings 
    declare @levelDiscountAmount decimal(15,6), @custLevelAllowsCoupons tinyint, @LevelAllowsQuantityDiscounts bit
    select @levelDiscountAmount = 0, @custLevelAllowsCoupons = 1, @LevelAllowsQuantityDiscounts = 1
    select @levelDiscountAmount = levelDiscountAmount*@exchangerate, @custLevelAllowsCoupons = LevelAllowsCoupons, @LevelAllowsQuantityDiscounts = LevelAllowsQuantityDiscounts  from dbo.customerlevel with (nolock) where CustomerLevelID = @customerlevel


    declare @DiscountPct decimal(15,6), @DiscountAmt decimal(15,6), @ValidCustomers nvarchar(4000), @ValidProducts nvarchar(4000), @ValidManufacturers nvarchar(4000), @ValidCategories nvarchar(4000), @ValidSections nvarchar(4000)
    select @DiscountPct = 0, @DiscountAmt = 0, @ValidCustomers = '', @ValidProducts = '', @ValidManufacturers = '', @ValidCategories = '', @ValidSections = ''
    declare @validcustomerscount int
    set @validcustomerscount = 0
    declare @couponproducts table (ProductID int)


    -- for estimator
    -- just check for the country
    Declare @CountryID int,@StateID int, @ZipCode nvarchar(50)
    if @Country is not null
    Begin
      SELECT @CountryID = CountryID FROM dbo.Country WHERE Name = @Country
      SELECT @StateID = StateID FROM dbo.[State] WHERE Abbreviation = @State
      SELECT @ZipCode = ZipCode FROM Ziptaxrate WHERE zipcode = @PostalCode
    End


    if @debug = 1 begin
        select @exchangerate exchangerate
    end

    --Get Product Level Coupon data
    if rtrim(@couponcode) <> '' and @includediscounts = 1 and @custLevelAllowsCoupons = 1 begin
        select 
            @DiscountPct = DiscountPercent, @DiscountAmt = DiscountAmount,
            @ValidCustomers = isnull(ValidForCustomers, ''), @ValidProducts = isnull(ValidForProducts, ''), @ValidManufacturers = isnull(ValidForManufacturers, ''), @ValidCategories = isnull(ValidForCategories, ''), @ValidSections = isnull(ValidForSections, '')
        from dbo.coupon with (nolock) 
        where couponcode = @couponcode and coupontype = 1 and ExpirationDate >= dateadd(dy, -1, getdate())

        select @validcustomerscount = count(*) from dbo.Split(@ValidCustomers, ',')

        insert @couponproducts 
        select p.productid from dbo.product p with (nolock) join dbo.Split(@ValidProducts, ',') vp on p.productid = cast(vp.items as int)
        union
        select pc.productid from dbo.productcategory pc with (nolock) join dbo.Split(@ValidCategories, ',') vc on pc.categoryid = cast(vc.items as int)
        union
        select pm.productid from dbo.productmanufacturer pm with (nolock) join dbo.Split(@ValidManufacturers, ',') vm on pm.manufacturerid = cast(vm.items as int)
        union
        select ps.productid from dbo.productsection ps with (nolock) join dbo.Split(@ValidSections, ',') vs on ps.sectionid = cast(vs.items as int)


        if @debug = 1 begin
            select @DiscountPct ProductLevelCouponDiscountPercent, @DiscountAmt DiscountAmount, @ValidCustomers ValidForCustomers, @ValidProducts ValidForProducts, @ValidManufacturers ValidForManufacturers, @ValidCategories ValidForCategories, @ValidSections ValidForSections
            select * From @couponproducts 
        end
    end


    declare @vatcountryid int
    select @vatcountryid = convert(int, configvalue) from dbo.appconfig with (nolock) where [name] = 'VAT.CountryID'


    declare @buysafecost money
    select @buysafecost = BuySafe FROM dbo.Customer with (nolock) WHERE CustomerID = @customerid

    declare @tblSubtotal1 table (ShoppingCartRecID int, CustomerID int, ShippingAddressID int, TaxAddressID int, ProductID int, TaxClassID int, ProductPrice decimal(15,6), Qty int, IsTaxable tinyint, IsSystem tinyint, IsDownload tinyint, FreeShipping tinyint, DiscountedPrice decimal(15,6), FinalDiscountedPrice decimal(15,6), Tax decimal(15,6), IsOrderOption bit)
    declare @tblSubtotal  table (ShoppingCartRecID int, CustomerID int, ShippingAddressID int, TaxAddressID int, ProductID int, TaxClassID int, ProductPrice decimal(15,6), Qty int, IsTaxable tinyint, IsSystem tinyint, IsDownload tinyint, FreeShipping tinyint, DiscountedPrice decimal(15,6), FinalDiscountedPrice decimal(15,6), Tax decimal(15,6), IsOrderOption bit)


    insert @tblSubtotal1
    select sc.ShoppingCartRecID, sc.CustomerID, sc.ShippingAddressID, case @taxcalcmode when 'billing' then sc.BillingAddressID else sc.ShippingAddressID end, sc.ProductID, p.TaxClassID, sc.ProductPrice , sc.Quantity, sc.IsTaxable, sc.IsSystem, sc.IsDownload, sc.FreeShipping, 
        dbo.ZeroFloor(dbo.ZeroFloor(100-((case @LevelAllowsQuantityDiscounts when 1 then dbo.GetQtyDiscount(sc.productid, a.quantity, 0) else 0 end+case when cp.productid is not null and (@validcustomerscount = 0 or cast(vc.items as int) = sc.CustomerID) then @DiscountPct else 0 end)))*convert(decimal(15,6), round(sc.ProductPrice*@exchangerate, 2))/100
        - dbo.ZeroFloor((case when cp.productid is not null and (@validcustomerscount = 0 or cast(vc.items as int) = sc.CustomerID) then  round(@DiscountAmt*@exchangerate, 2) else 0 end) + case @LevelAllowsQuantityDiscounts when 1 then round(dbo.GetQtyDiscount(sc.productid, a.quantity, 1)*@exchangerate, 2) else 0 end)) DiscountedPrice,
        0,
        0,
        0
    from dbo.shoppingcart sc with (nolock) 
        join dbo.product p with (nolock) on sc.ProductID = p.ProductID
        left join dbo.Split(@ValidCustomers, ',') vc on sc.customerid = cast(vc.items as int)
        left join @couponproducts cp on sc.ProductID = cp.productid
        left join (select ProductID, sum(Quantity) Quantity FROM dbo.shoppingcart with (nolock) where customerid = @customerid and CartType = @CartType and (@OriginalRecurringOrderNumber = 0 or OriginalRecurringOrderNumber = @OriginalRecurringOrderNumber) and (@OnlyLoadRecurringItemsThatAreDue = 0 or (@CartType = 2 and NextRecurringShipDate < dateadd(dy, 1, getdate())))   group by ProductID) a on sc.ProductID = a.ProductID
    where sc.customerid = @customerid  
        and sc.CustomerID = isnull(cast(vc.items as int), sc.CustomerID)
        and sc.CartType = @CartType 
        and (@OriginalRecurringOrderNumber = 0 or sc.OriginalRecurringOrderNumber = @OriginalRecurringOrderNumber)    
        and (@OnlyLoadRecurringItemsThatAreDue = 0 or (@CartType = 2 and NextRecurringShipDate < dateadd(dy, 1, getdate())))
    UNION
    select -1, @customerid, @DefaultShippingAddressID, @custtaxingaddressid, o.OrderOptionID, TaxClassID, (case when lower(o.Name)='buysafe' then @buysafecost else Cost end) as Cost, 1, 1, 0, 0, 0, (case when lower(o.Name)='buysafe' then @buysafecost else Cost end) as Cost, 0, 0, 1 From dbo.orderoption o with (nolock) join dbo.Split(@orderoptions, ',') a on o.OrderoptionID = cast(a.Items as int)




    insert @tblSubtotal
    select * from @tblSubtotal1 where (ShippingAddressID = @AddressID or @AddressID = 0)




    if @debug = 1
        select * From @tblSubtotal




    DECLARE @subtotal decimal(15,6), @CartQty int
    select @subtotal = sum(DiscountedPrice*Qty), @CartQty  = sum(Qty) from @tblSubtotal1

    select @DiscountPct = 0, @DiscountAmt = 0, @ValidCustomers = '', @ValidProducts = '', @ValidManufacturers = '', @ValidCategories = '', @ValidSections = ''


    declare @validproductcount int
    set @validproductcount = 0

    if @includediscounts = 1 begin 

        if rtrim(@couponcode) <> '' and @custLevelAllowsCoupons = 1  begin

            select
                @DiscountPct = DiscountPercent, @DiscountAmt = DiscountAmount*@exchangerate,
                @ValidCustomers = isnull(ValidForCustomers, ''), @ValidProducts = isnull(ValidForProducts, ''), @ValidManufacturers = isnull(ValidForManufacturers, ''), @ValidCategories = isnull(ValidForCategories, ''), @ValidSections = isnull(ValidForSections, '')
            from dbo.coupon with (nolock) 
            where couponcode = @couponcode and coupontype = 0 and ExpirationDate >= dateadd(dy, -1, getdate())


            --Get Order level discounts
            delete @couponproducts 

            declare @validproductlist int
            insert @couponproducts 
            select p.productid from dbo.product p with (nolock) join dbo.Split(@ValidProducts, ',') vp on p.productid = cast(vp.items as int)
            union
            select pc.productid from dbo.productcategory pc with (nolock) join dbo.Split(@ValidCategories, ',') vc on pc.categoryid = cast(vc.items as int)
            union
            select pm.productid from dbo.productmanufacturer pm with (nolock) join dbo.Split(@ValidManufacturers, ',') vm on pm.manufacturerid = cast(vm.items as int)
            union
            select ps.productid from dbo.productsection ps with (nolock) join dbo.Split(@ValidSections, ',') vs on ps.sectionid = cast(vs.items as int)

            set @validproductlist = @@rowcount


            set @validcustomerscount = -1
            select @validcustomerscount = count(*) from dbo.Split(@ValidCustomers, ',') where cast(items as int) = @customerid

            select @validproductcount = count(*) from @tblsubtotal s join @couponproducts cp on s.ProductID = cp.productid

            if @debug = 1
                select 'Order level Coupon Discounts', @DiscountPct, @DiscountAmt, @levelDiscountAmount, @subtotal, @validcustomerscount, @validproductcount

            -- if customers are specified but none found or products specified but none found then set the discount to zero.
            if (rtrim(@ValidCustomers) <> '' and @validcustomerscount = 0) or (@validproductlist <> 0 and @validproductcount = 0)
                select @DiscountPct = 0, @DiscountAmt = 0
        end




        declare @OrderDiscountPct decimal(15,6)
        if( @subtotal > 0 ) begin
                SET @OrderDiscountPct = ((@levelDiscountAmount+@DiscountAmt)/@subtotal) + (@DiscountPct/100)
        end
        else begin
                SET @OrderDiscountPct = 0
        end





        -- Calculate discount based on number of items in cart 
        declare @discountoverNitems int
        set @discountoverNitems  = 0
        select @discountoverNitems = case when isnumeric(configvalue) = 1 then floor(configvalue) else 0 end from dbo.appconfig with (nolock) where [name] = 'DiscountCartIfOverNItems'

        declare @discountoverNitemsPct decimal(15,6)
        set @discountoverNitemsPct  = 0
        select @discountoverNitemsPct = case when @CartQty > @discountoverNitems and isnumeric(configvalue) = 1 then cast(configvalue as decimal(15,6)) else 0 end from dbo.appconfig with (nolock) where [name] = 'DiscountCartIfOverNItemsDiscountPercentage'

        set @OrderDiscountPct = @OrderDiscountPct + (@discountoverNitemsPct/100)


        UPDATE @tblsubtotal
        SET FinalDiscountedPrice = dbo.ZeroFloor(round((DiscountedPrice*(1-@OrderDiscountPct)), 6))

    end 
    else begin
        UPDATE @tblsubtotal
        SET FinalDiscountedPrice = DiscountedPrice
    end



    -- Calculate item Tax
    declare @vatroundingmethod int
    select @vatroundingmethod = case configvalue when 'true' then 1 else 0 end from dbo.appconfig with (nolock) where [name] = 'VAT.RoundPerItem'


    UPDATE @tblsubtotal
    SET Tax = CASE sc.IsTaxable  
                WHEN 1 THEN 
                    case @vatroundingmethod     
                      when 1 then round(FinalDiscountedPrice*((isnull(cr.taxrate, 0)+isnull(sr.taxrate, 0)+isnull(zr.taxrate, 0))/100), 2)*Qty    
                      else round((FinalDiscountedPrice*Qty)*((isnull(cr.taxrate, 0)+isnull(sr.taxrate, 0)+isnull(zr.taxrate, 0))/100), 2)    
                      end 
                ELSE 0 
               END
    FROM @tblsubtotal sc
        left join dbo.address a with (nolock) on sc.TaxAddressID = a.addressid
        left join dbo.country c with (nolock) on c.name = a.country
        left join dbo.state s with (nolock) on s.abbreviation = a.state and s.countryid = c.countryid
        left join dbo.countrytaxrate cr with (nolock) on cr.countryid = 
                                 CASE
                                 WHEN @CountryID is not null then   -- get the pass country
                                          @CountryID
                                 ELSE                               -- use countryid or the vat country id  
                                      isnull(c.countryid, @vatcountryid) 
                                 END
        and cr.TaxClassID = sc.TaxCLassID
        left join dbo.statetaxrate sr with (nolock) on sr.stateid = 
                                 CASE
                                 WHEN @StateID is not null then   -- get the pass state
                                          @StateID
                                 ELSE                               -- use state 
                                      s.StateID 
                                 END
        
        and sr.TaxClassID = sc.TaxCLassID
        left join dbo.ZipTaxRate zr with (nolock) on zr.ZipCode = 
                                 CASE
                                 WHEN @ZipCode is not null then   -- get the pass state
                                          @PostalCode
                                 ELSE                               -- use state 
                                      a.Zip 
                                 END
        and zr.TaxClassID = sc.TaxCLassID



    if @debug = 1
        select *, @discountoverNitems discountoverNitems, @OrderDiscountPct OrderDiscountPct from @tblsubtotal

    select 
        round(sum(FinalDiscountedPrice*Qty)+sum(case when @IncludeTaxInSubTotal = 1 then Tax else 0 end), 2) SubTotal,
        round(sum(Tax), 2) TotalTax 
    from @tblsubtotal
    where IsTaxable >= @IncludeOnlyTaxable 
        and isnull(IsSystem, 0) <= @includeSystemItems 
        and IsDownload <= @includeDownloadItems 
        and FreeShipping <= @includeFreeShippingItems



    GO





















    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_WorldShipExport]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_WorldShipExport]
    GO

    CREATE proc dbo.aspdnsf_WorldShipExport  
        WITH ENCRYPTION         
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





    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_delTaxClass]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_delTaxClass]
    GO
    create proc [dbo].[aspdnsf_delTaxClass]
        @TaxClassID int
        WITH ENCRYPTION
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





    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_getTaxclass]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_getTaxclass]
    GO
    create proc [dbo].[aspdnsf_getTaxclass]
        @TaxClassID int = null
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 

    SELECT TaxClassID, TaxClassGUID, Name, TaxCode, DisplayOrder, CreatedOn
    FROM dbo.Taxclass with (nolock) 
    WHERE TaxClassID = COALESCE(@TaxClassID, TaxClassID)

    GO







    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_updTaxclass]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_updTaxclass]
    GO
    create proc [dbo].[aspdnsf_updTaxclass]
        @TaxClassID int,
        @TaxCode nvarchar(400),
        @DisplayOrder int
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    UPDATE dbo.Taxclass
    SET 
        TaxCode = COALESCE(@TaxCode, TaxCode),
        DisplayOrder = COALESCE(@DisplayOrder, DisplayOrder)
    WHERE TaxClassID = @TaxClassID



    GO




    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_insTaxclass]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[aspdnsf_insTaxclass]
    GO
    create proc [dbo].[aspdnsf_insTaxclass]
        @Name nvarchar(400),
        @TaxCode nvarchar(400),
        @DisplayOrder int,
        @TaxClassID int OUTPUT
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    insert dbo.Taxclass(TaxClassGUID, Name, TaxCode, DisplayOrder, CreatedOn)
    values (newid(), @Name, @TaxCode, @DisplayOrder, getdate())

    set @TaxClassID = @@identity

    GO


    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_updZipTaxRate]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_updZipTaxRate]
    GO
    create proc [dbo].[aspdnsf_updZipTaxRate]
        @ZipTaxID int,
        @TaxRate money
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    UPDATE dbo.ZipTaxRate
    SET 
        TaxRate = COALESCE(@TaxRate, TaxRate)
    WHERE ZipTaxID = @ZipTaxID
    GO


    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_CreateSubEntities]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_CreateSubEntities]
    GO
    create proc dbo.aspdnsf_CreateSubEntities
        @EntityName varchar(20),
        @EntityID int,
        @EntityList varchar(8000)
        WITH ENCRYPTION
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


    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_insZipTaxRate]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_insZipTaxRate]
    GO
    create proc [dbo].[aspdnsf_insZipTaxRate]
        @ZipCode nvarchar(10),
        @TaxClassID int,
        @TaxRate money,
        @ZipTaxID int OUTPUT
    WITH ENCRYPTION    
    AS
    SET NOCOUNT ON 


    insert dbo.ZipTaxRate(ZipCode, TaxClassID, TaxRate, CreatedOn)
    values (@ZipCode, @TaxClassID, @TaxRate, getdate())

    set @ZipTaxID = @@identity



    GO
    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_getZipTaxRate]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_getZipTaxRate]
    GO
    create proc [dbo].[aspdnsf_getZipTaxRate]
        @ZipCode nvarchar(10) = null,
        @TaxClassID int = null
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    SELECT ztr.ZipTaxID, ztr.ZipCode, ztr.TaxClassID, ztr.TaxRate, ztr.CreatedOn, t.Name TaxClass
    FROM dbo.ZipTaxRate ztr with (nolock) join dbo.TaxClass t with (nolock) on ztr.TaxClassID = t.TaxClassID 
    WHERE ztr.ZipCode = COALESCE(@ZipCode, ztr.ZipCode) and ztr.TaxClassID = COALESCE(@TaxClassID, ztr.TaxClassID)

    GO


    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_getZipTaxRateByID]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_getZipTaxRateByID]
    GO
    create proc [dbo].[aspdnsf_getZipTaxRateByID]
        @ZipTaxID int
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    SELECT ztr.ZipTaxID, ztr.ZipCode, ztr.TaxClassID, ztr.TaxRate, ztr.CreatedOn, t.Name TaxClass
    FROM dbo.ZipTaxRate ztr with (nolock) join dbo.TaxClass t with (nolock) on ztr.TaxClassID = t.TaxClassID 
    WHERE ztr.ZipTaxID = @ZipTaxID 

    GO




    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_updStateTaxRate]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_updStateTaxRate]
    GO
    create proc [dbo].[aspdnsf_updStateTaxRate]
        @StateTaxID int,
        @TaxRate money
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    UPDATE dbo.StateTaxRate
    SET 
        TaxRate = COALESCE(@TaxRate, TaxRate)
    WHERE StateTaxID = @StateTaxID



    GO
    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_insStateTaxRate]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[aspdnsf_insStateTaxRate]
    GO
    create proc [dbo].[aspdnsf_insStateTaxRate]
        @StateID int,
        @TaxClassID int,
        @TaxRate money,
        @StateTaxID int OUTPUT
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    insert dbo.StateTaxRate(StateID, TaxClassID, TaxRate, CreatedOn)
    values (@StateID, @TaxClassID, @TaxRate, getdate())

    set @StateTaxID = @@identity


    GO




    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_getStateTaxRateByID]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_getStateTaxRateByID]
    GO
    create proc [dbo].[aspdnsf_getStateTaxRateByID]
        @StateTaxID int
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    SELECT sr.StateTaxID, sr.StateID, sr.TaxClassID, sr.TaxRate, sr.CreatedOn, t.Name TaxClass, s.Name StateName
    FROM dbo.StateTaxRate sr with (nolock) join dbo.TaxClass t with (nolock) on sr.TaxClassID = t.TaxClassID join dbo.State s on s.StateID = sr.StateID
    WHERE StateTaxID = @StateTaxID

    GO





    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_getStateTaxRate]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_getStateTaxRate]
    GO
    create proc [dbo].[aspdnsf_getStateTaxRate]
        @StateID int = null,
        @TaxClassID int = null
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    SELECT sr.StateTaxID, sr.StateID, sr.TaxClassID, sr.TaxRate, sr.CreatedOn, t.Name TaxClass, s.Name StateName
    FROM dbo.StateTaxRate sr with (nolock) join dbo.TaxClass t with (nolock) on sr.TaxClassID = t.TaxClassID join dbo.State s on s.StateID = sr.StateID
    WHERE sr.StateID = COALESCE(@StateID, sr.StateID) and sr.TaxClassID = COALESCE(@TaxClassID, sr.TaxClassID)

    GO





    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_updCountryTaxRate]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_updCountryTaxRate]
    GO
    create proc [dbo].[aspdnsf_updCountryTaxRate]
        @CountryTaxID int,
        @TaxRate money
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    UPDATE dbo.CountryTaxRate
    SET 
        TaxRate = COALESCE(@TaxRate, TaxRate)
    WHERE CountryTaxID = @CountryTaxID


    GO




    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_getCountryTaxRateByID]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_getCountryTaxRateByID]
    GO
    create proc [dbo].[aspdnsf_getCountryTaxRateByID]
        @CountryTaxID int
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    SELECT ctr.CountryTaxID, ctr.CountryID, ctr.TaxClassID, ctr.TaxRate, ctr.CreatedOn, t.Name TaxClass, c.Name Country
    FROM dbo.CountryTaxRate ctr with (nolock) join dbo.TaxClass t with (nolock) on ctr.TaxClassID = t.TaxClassID join dbo.Country c on c.CountryID = ctr.CountryID
    WHERE ctr.CountryTaxID = @CountryTaxID

    GO




    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_getCountryTaxRate]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_getCountryTaxRate]
    GO
    create proc [dbo].[aspdnsf_getCountryTaxRate]
        @CountryID int = null,
        @TaxClassID int = null
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    SELECT ctr.CountryTaxID, ctr.CountryID, ctr.TaxClassID, ctr.TaxRate, ctr.CreatedOn, t.Name TaxClass, c.Name Country
    FROM dbo.CountryTaxRate ctr with (nolock) join dbo.TaxClass t with (nolock) on ctr.TaxClassID = t.TaxClassID join dbo.Country c on c.CountryID = ctr.CountryID
    WHERE ctr.CountryID = COALESCE(@CountryID, ctr.CountryID) and ctr.TaxClassID = COALESCE(@TaxClassID, ctr.TaxClassID)

    GO

    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_insCountryTaxRate]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_insCountryTaxRate]
    GO
    create proc [dbo].[aspdnsf_insCountryTaxRate]
        @CountryID int,
        @TaxClassID int,
        @TaxRate money,
        @CountryTaxID int OUTPUT
        WITH ENCRYPTION

    AS
    SET NOCOUNT ON 


    insert dbo.CountryTaxRate(CountryID, TaxClassID, TaxRate, CreatedOn)
    values (@CountryID, @TaxClassID, @TaxRate, getdate())

    set @CountryTaxID = @@identity

    go



    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_insCustomer]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_insCustomer]
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
        WITH ENCRYPTION
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



    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_insAffiliate]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_insAffiliate]
    go
    create proc dbo.aspdnsf_insAffiliate
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
        @AffiliateID int OUTPUT
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    insert dbo.Affiliate(AffiliateGUID, EMail, Password, DateOfBirth, Gender, Notes, IsOnline, FirstName, LastName, [Name], Company, Address1, Address2, Suite, City, State, Zip, Country, Phone, WebSiteName, WebSiteDescription, URL, TrackingOnly, DefaultSkinID, ParentAffiliateID, DisplayOrder, ExtensionData, SEName, SETitle, SENoScript, SEAltText, SEKeywords, SEDescription, Published, Wholesale, CreatedOn, SaltKey)
    values (newid(), @EMail, @Password, @DateOfBirth, @Gender, @Notes, @IsOnline, @FirstName, @LastName, @Name, @Company, @Address1, @Address2, @Suite, @City, @State, @Zip, @Country, @Phone, @WebSiteName, @WebSiteDescription, @URL, @TrackingOnly, @DefaultSkinID, @ParentAffiliateID, @DisplayOrder, @ExtensionData, @SEName, @SETitle, @SENoScript, @SEAltText, @SEKeywords, @SEDescription, 1, @Wholesale, getdate(), @SaltKey)

    set @AffiliateID = @@identity

    go



     
     
    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_getAffiliate]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_getAffiliate]
    go
    create proc dbo.aspdnsf_getAffiliate
        @AffiliateID int = null
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    SELECT AffiliateID, AffiliateGUID, EMail, Password, DateOfBirth, Gender, Notes, IsOnline, FirstName, LastName, Name, Company, Address1, Address2, Suite, City, State, Zip, Country, Phone, WebSiteName, WebSiteDescription, URL, TrackingOnly, DefaultSkinID, ParentAffiliateID, DisplayOrder, ExtensionData, SEName, SETitle, SENoScript, SEAltText, SEKeywords, SEDescription, Published, Wholesale, Deleted, CreatedOn, SaltKey
    FROM dbo.Affiliate with (nolock) 
    WHERE AffiliateID = COALESCE(@AffiliateID, AffiliateID)

    go
     
     
     

     
    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_getAffiliateByEmail]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_getAffiliateByEmail]
    go
    create proc dbo.aspdnsf_getAffiliateByEmail
        @AffiliateEmail nvarchar(100)
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    SELECT AffiliateID, AffiliateGUID, EMail, Password, DateOfBirth, Gender, Notes, IsOnline, FirstName, LastName, Name, Company, Address1, Address2, Suite, City, State, Zip, Country, Phone, WebSiteName, WebSiteDescription, URL, TrackingOnly, DefaultSkinID, ParentAffiliateID, DisplayOrder, ExtensionData, SEName, SETitle, SENoScript, SEAltText, SEKeywords, SEDescription, Published, Wholesale, Deleted, CreatedOn, SaltKey
    FROM dbo.Affiliate with (nolock) 
    WHERE EMail = @AffiliateEmail

    go
     
     
     

    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_insAppconfig]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_insAppconfig]
    go
    create proc dbo.aspdnsf_insAppconfig
        @Name nvarchar(100),
        @Description ntext,
        @ConfigValue nvarchar(1000),
        @GroupName nvarchar(100),
        @SuperOnly tinyint,
        @AppConfigID int OUTPUT
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


        INSERT dbo.Appconfig(AppConfigGUID, Name, Description, ConfigValue, GroupName, SuperOnly, CreatedOn)
        VALUES (newid(), @Name, @Description, @ConfigValue, @GroupName, @SuperOnly, getdate())

        set @AppConfigID = @@identity

    go


    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_getAppconfig]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_getAppconfig]
    go
    create proc dbo.aspdnsf_getAppconfig
        @AppConfigID int = null
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 

        SELECT AppConfigID, AppConfigGUID, [Name], Description, ConfigValue, GroupName, SuperOnly, CreatedOn
        FROM dbo.Appconfig with (nolock) 
        WHERE AppConfigID = COALESCE(@AppConfigID, AppConfigID)
        ORDER BY [Name]

    go



    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_SecurityLogInsert]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_SecurityLogInsert]
    go
    create proc [dbo].[aspdnsf_SecurityLogInsert]
        @SecurityAction nvarchar(100),
        @Description ntext,
        @CustomerUpdated int,
        @UpdatedBy int,
        @CustomerSessionID int,
        @logid bigint OUTPUT
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    insert dbo.SecurityLog(SecurityAction, Description, ActionDate, CustomerUpdated, UpdatedBy, CustomerSessionID)
    values (@SecurityAction, @Description, getdate(), @CustomerUpdated, @UpdatedBy, @CustomerSessionID)

    set @logid = @@identity

    go



    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_GetShoppingCart]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_GetShoppingCart]
    GO
    create proc [dbo].[aspdnsf_GetShoppingCart]
        @CartType tinyint, -- ShoppingCart = 0, WishCart = 1, RecurringCart = 2, GiftRegistryCart = 3
        @CustomerID int,
        @OriginalRecurringOrderNumber int,
        @OnlyLoadRecurringItemsThatAreDue tinyint
        WITH ENCRYPTION
    AS
    BEGIN
     
        SET NOCOUNT ON 
     
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
     
        WHERE ShoppingCart.CartType = @CartType
            and Product.Deleted in (0,2)
            and ProductVariant.Deleted = 0
            and Customer.customerid = @CustomerID
            and (@OriginalRecurringOrderNumber = 0 or ShoppingCart.OriginalRecurringOrderNumber = @OriginalRecurringOrderNumber)
            and (@OnlyLoadRecurringItemsThatAreDue = 0 or (@CartType = 2 and NextRecurringShipDate < dateadd(dy, 1, getdate())))
         ORDER BY ShoppingCart.GiftRegistryForCustomerID,ShoppingCart.ShippingAddressID
     
    END
     
    GO
     
     
     






    -- must add this proc first:
    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_CustomerConsistencyCheck]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_CustomerConsistencyCheck]
    go
    create proc dbo.aspdnsf_CustomerConsistencyCheck
        WITH ENCRYPTION
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
    go




    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_CreateGiftCard]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_CreateGiftCard]
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
        WITH ENCRYPTION
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










    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_updGiftCard]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_updGiftCard]
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
        WITH ENCRYPTION
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





    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_insGiftCardUsage]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_insGiftCardUsage]
    GO

    create proc [dbo].[aspdnsf_insGiftCardUsage]
        @GiftCardID int,
        @UsageTypeID int,
        @UsedByCustomerID int,
        @OrderNumber int,
        @Amount money,
        @ExtensionData ntext = null,
        @GiftCardUsageID int OUTPUT
        WITH ENCRYPTION
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











    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_updGiftCardUsage]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_updGiftCardUsage]
    GO
    create proc [dbo].[aspdnsf_updGiftCardUsage]
        @GiftCardUsageID int,
        @UsageTypeID int,
        @UsedByCustomerID int,
        @OrderNumber int,
        @Amount money,
        @ExtensionData ntext
        WITH ENCRYPTION
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
    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_getGiftCardUsage]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_getGiftCardUsage]
    GO
    create proc [dbo].[aspdnsf_getGiftCardUsage]
        @GiftCardUsageID int
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    SELECT GiftCardUsageID, GiftCardUsageGUID, GiftCardID, UsageTypeID, UsedByCustomerID, OrderNumber, Amount, ExtensionData, CreatedOn
    FROM dbo.GiftCardUsage with (nolock) 
    WHERE GiftCardUsageID = COALESCE(@GiftCardUsageID, GiftCardUsageID)


    GO



    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_getGiftCardUsageByGiftCard]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_getGiftCardUsageByGiftCard]
    GO
    create proc [dbo].[aspdnsf_getGiftCardUsageByGiftCard]
        @GiftCardID int
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 


    SELECT GiftCardUsageID, GiftCardUsageGUID, GiftCardID, UsageTypeID, UsedByCustomerID, OrderNumber, Amount, ExtensionData, CreatedOn
    FROM dbo.GiftCardUsage with (nolock) 
    WHERE GiftCardID = @GiftCardID

    GO



    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_SearchProductComments]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_SearchProductComments]
    go
    create proc dbo.aspdnsf_SearchProductComments
        @Search varchar(1000) = '',
        @CustomerID int = null,
        @FilthyOnly tinyint = 0,
        @Sort tinyint = 1,
        @days int = 7
        WITH ENCRYPTION
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









    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_NukeProduct]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_NukeProduct]
    GO
    create proc [dbo].[aspdnsf_NukeProduct]
        @ProductID int
        WITH ENCRYPTION
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






    if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_PABPEraseCCInfo') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
        drop proc [dbo].[aspdnsf_PABPEraseCCInfo]
    go
    create proc [dbo].[aspdnsf_PABPEraseCCInfo]
        @CartType int
        WITH ENCRYPTION
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













    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_MonthlyMaintenance]') AND type in (N'P', N'PC'))
            DROP PROCEDURE [dbo].[aspdnsf_MonthlyMaintenance]
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
        WITH ENCRYPTION
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


    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_GetProducts]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_GetProducts]
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
    @IncludeAll      bit = 0 -- Don't filter products that have a start date in the future or a stop date in the past  
        WITH ENCRYPTION
    AS  
    BEGIN  
      
        SET NOCOUNT ON   
      
        DECLARE @rcount int  
        DECLARE @productfilter table (rownum int not null identity  primary key, productid int not null, displayorder int not null, VariantID int not null, VariantDisplayOrder int not null, ProductName nvarchar(400) null, VariantName nvarchar(400) null)  
        DECLARE @FilterProductsByAffiliate tinyint, @FilterProductsByCustomerLevel tinyint, @DisplayOutOfStockProducts tinyint, @HideProductsWithLessThanThisInventoryLevel int  
        CREATE TABLE #displayorder (productid int not null primary key, displayorder int not null)  
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
            INSERT #displayorder select productid, displayorder from dbo.ProductCategory with (nolock) where categoryID = @categoryID  
        END  
        ELSE IF @sortEntity = 2 or @sortEntityName = 'section' BEGIN  
            INSERT #displayorder select productid, displayorder from dbo.ProductSection with (nolock) where sectionId = @sectionID  
        END  
        ELSE IF @sortEntity = 3 or @sortEntityName = 'manufacturer' BEGIN  
            INSERT #displayorder select productid, displayorder from dbo.ProductManufacturer with (nolock) where ManufacturerID = @manufacturerID  
        END  
        ELSE IF @sortEntity = 4 or @sortEntityName = 'distributor' BEGIN  
            INSERT #displayorder select productid, displayorder from dbo.ProductDistributor with (nolock) where DistributorID = @distributorID  
        END  
        ELSE IF @sortEntity = 5 or @sortEntityName = 'genre' BEGIN  
            INSERT #displayorder select productid, displayorder from dbo.ProductGenre with (nolock) where GenreID = @genreID  
        END  
        ELSE IF @sortEntity = 6 or @sortEntityName = 'vector' BEGIN  
            INSERT #displayorder select productid, displayorder from dbo.ProductVector with (nolock) where VectorID = @vectorID  
        END  
        ELSE BEGIN  
            INSERT #displayorder select productid, 1 from dbo.Product with (nolock) ORDER BY Name  
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











    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_ProductInfo]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_ProductInfo]
    GO
    create proc [dbo].[aspdnsf_ProductInfo]
        @ProductID          int,  
        @CustomerLevelID    int,  
        @DefaultVariantOnly tinyint,  
        @InvFilter          int = 0,  
        @affiliateID        int = null,  
        @publishedonly      tinyint = 1  
        WITH ENCRYPTION
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






    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_EditOrderProduct]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_EditOrderProduct]
    GO
    create proc [dbo].[aspdnsf_EditOrderProduct]
        @ShoppingCartRecID int,
        @CustomerLevelID   int
        WITH ENCRYPTION
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







    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_CloneVariant]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_CloneVariant]
    GO
    CREATE proc [dbo].[aspdnsf_CloneVariant]
        @VariantID int,
        @userid int = 0 
        WITH ENCRYPTION
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


    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_SearchCategories]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_SearchCategories]
    GO
    create proc [dbo].[aspdnsf_SearchCategories]
        @SearchTerm nvarchar(3000),
        @CategoryID int = null
        WITH ENCRYPTION
    AS
    BEGIN
    SET NOCOUNT ON

        SET @SearchTerm = '%' + rtrim(ltrim(@SearchTerm)) + '%'
        SET @CategoryID = nullif(@CategoryID, 0)

        SELECT * 
        FROM dbo.Category   with (NOLOCK)  
        WHERE category.name like @SearchTerm 
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


    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_ProductSequence]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_ProductSequence]
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
        WITH ENCRYPTION
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
            left join ProductVariant pv		   with (nolock) on p.ProductID = pv.ProductID
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

    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_AdjustInventory]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_AdjustInventory]
    GO
    create proc [dbo].[aspdnsf_AdjustInventory]
        @ordernumber int,
        @direction smallint -- 1 = add items to inventory, -1 = remove from inventory
        WITH ENCRYPTION
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

    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_CreateDefaultVariant]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_CreateDefaultVariant]
    GO
    create proc [dbo].[aspdnsf_CreateDefaultVariant]
        WITH ENCRYPTION
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





    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_ExportProductList]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_ExportProductList]
    GO
    create proc [dbo].[aspdnsf_ExportProductList]
        @categoryID int = -1,
        @sectionID int = -1,
        @manufacturerID int = -1,
        @distributorID int = -1,
        @genreID int = -1,
        @vectorID int = -1
    WITH ENCRYPTION    
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


    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_SearchSections]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_SearchSections]
    GO
    create proc [dbo].[aspdnsf_SearchSections]
        @SearchTerm nvarchar(3000),
        @SectionID int = null
        WITH ENCRYPTION
    AS
    BEGIN
    SET NOCOUNT ON

        SET @SearchTerm = '%' + rtrim(ltrim(@SearchTerm)) + '%'
        SET @SectionID = nullif(@SectionID, 0)


        SELECT * 
        FROM dbo.[Section]  with (NOLOCK)  
        WHERE Deleted=0 and Published=1 
            and SectionID = coalesce(@SectionID, SectionID)
            and [Section].Name like @SearchTerm
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


    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_CloneProduct]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_CloneProduct]
    GO
    create proc [dbo].[aspdnsf_CloneProduct]
        @productID int,
        @userid int = 0, 
        @cloneinventory int = 1
        WITH ENCRYPTION
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


    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_ClearAllImportFlags]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_ClearAllImportFlags]
    GO
    create procEDURE [dbo].[aspdnsf_ClearAllImportFlags]
        WITH ENCRYPTION
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

    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_UndoImport]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_UndoImport]
    GO
    create procEDURE [dbo].[aspdnsf_UndoImport]
        WITH ENCRYPTION
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






    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_EntityMgr]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_EntityMgr]
    GO
    create proc [dbo].[aspdnsf_EntityMgr]
        @EntityName varchar(100),
        @PublishedOnly tinyint
        WITH ENCRYPTION
    AS
    BEGIN
        SET NOCOUNT ON
        IF @EntityName = 'Category' BEGIN
            SELECT Entity.CategoryID EntityID, Entity.CategoryGUID EntityGuid, Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentCategoryID ParentEntityID,DisplayOrder,SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor,GraphicsColor,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
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
            SELECT Entity.AffiliateID EntityID,Entity.AffiliateGUID EntityGuid, Name,4 as ColWidth,'' as Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentAffiliateID ParentEntityID,DisplayOrder,0 as SortByLooks,'' as XmlPackage,Published,'' as ContentsBGColor,'' as PageBGColor,'' as GraphicsColor,isnull(NumProducts, 0) NumObjects, 0 PageSize, 0 QuantityDiscountID, '' Summary, SkinID, TemplateName
            FROM dbo.Affiliate Entity with (NOLOCK)
              left join (SELECT pa.AffiliateID, COUNT(pa.ProductID) AS NumProducts
                         FROM dbo.ProductAffiliate pa with (nolock) join [dbo].Product p with (nolock) on pa.ProductID = p.ProductID and p.deleted=0 and p.published=1
                         GROUP BY pa.AffiliateID
                        ) a on Entity.AffiliateID = a.AffiliateID
            WHERE Published >= @PublishedOnly and Deleted=0
            ORDER BY ParentAffiliateID, DisplayOrder,Name
        END
     
     
     
        IF @EntityName = 'Section' BEGIN
            SELECT Entity.SectionID EntityID,Entity.SectionGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentSectionID ParentEntityID,DisplayOrder,SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor,GraphicsColor,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
            FROM dbo.Section Entity with (NOLOCK)
                left join (SELECT ps.SectionID, COUNT(ps.ProductID) AS NumProducts
                           FROM dbo.ProductSection ps with (nolock) join [dbo].Product p with (nolock) on ps.ProductID = p.ProductID and p.deleted=0 and p.published=1
                           GROUP BY ps.SectionID
                          ) a on Entity.SectionID = a.SectionID
            WHERE Published >= @PublishedOnly and Deleted=0
            ORDER BY ParentSectionID,DisplayOrder,Name
        END
     
     
     
        IF @EntityName = 'Manufacturer' BEGIN
            SELECT Entity.ManufacturerID EntityID,Entity.ManufacturerGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentManufacturerID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor,GraphicsColor,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
            FROM dbo.Manufacturer Entity with (NOLOCK)
            left join (SELECT pm.ManufacturerID, COUNT(pm.ProductID) AS NumProducts
                       FROM dbo.ProductManufacturer pm with (nolock) join [dbo].Product p with (nolock) on pm.ProductID = p.ProductID and p.deleted=0 and p.published=1
                       GROUP BY pm.ManufacturerID
                      ) a on Entity.ManufacturerID = a.ManufacturerID
            WHERE Published >= @PublishedOnly and Deleted=0
            ORDER BY ParentManufacturerID,DisplayOrder,Name
        END
     
     
     
        IF @EntityName = 'Library' BEGIN
            SELECT Entity.LibraryID EntityID,Entity.LibraryGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentLibraryID ParentEntityID,DisplayOrder,SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor,GraphicsColor,isnull(NumDocuments, 0) NumObjects, PageSize, 0 QuantityDiscountID, Summary, SkinID, TemplateName
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
            SELECT Entity.DistributorID EntityID,Entity.DistributorGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentDistributorID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor,GraphicsColor,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
            FROM dbo.Distributor Entity with (NOLOCK)
                left join (SELECT pd.DistributorID, COUNT(pd.ProductID) AS NumProducts
                           FROM dbo.ProductDistributor pd with (nolock) join [dbo].Product p with (nolock) on pd.ProductID = p.ProductID and p.deleted=0 and p.published=1
                           GROUP BY pd.DistributorID
                          ) a on Entity.DistributorID = a.DistributorID
            WHERE Published >= @PublishedOnly and Deleted=0
            ORDER BY ParentDistributorID,DisplayOrder,Name
        END
     
        IF @EntityName = 'Genre' BEGIN
            SELECT Entity.GenreID EntityID,Entity.GenreGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentGenreID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor,GraphicsColor,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
            FROM dbo.Genre Entity with (NOLOCK)
                left join (SELECT px.GenreID, COUNT(px.ProductID) AS NumProducts
                           FROM dbo.ProductGenre px with (nolock) join [dbo].Product p with (nolock) on px.ProductID = p.ProductID and p.deleted=0 and p.published=1
                           GROUP BY px.GenreID
                          ) a on Entity.GenreID = a.GenreID
            WHERE Published >= @PublishedOnly and Deleted=0
            ORDER BY ParentGenreID,DisplayOrder,Name
        END
     
        IF @EntityName = 'Vector' BEGIN
            SELECT Entity.VectorID EntityID,Entity.VectorGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentVectorID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor,GraphicsColor,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
            FROM dbo.Vector Entity with (NOLOCK)
                left join (SELECT px2.VectorID, COUNT(px2.ProductID) AS NumProducts
                           FROM dbo.ProductVector px2 with (nolock) join [dbo].Product p with (nolock) on px2.ProductID = p.ProductID and p.deleted=0 and p.published=1
                           GROUP BY px2.VectorID
                          ) a on Entity.VectorID = a.VectorID
            WHERE Published >= @PublishedOnly and Deleted=0
            ORDER BY ParentVectorID,DisplayOrder,Name
        END
     
     
        IF @EntityName = 'Customerlevel' BEGIN
            SELECT Entity.CustomerLevelID EntityID,Entity.CustomerLevelGUID EntityGuid,Name, 4 ColWidth, '' Description,SEName, '' SEKeywords, '' SEDescription, '' SETitle, '' SENoScript,'' SEAltText,ParentCustomerLevelID ParentEntityID,DisplayOrder,0 SortByLooks, '' XmlPackage, 1 Published,'' ContentsBGColor, '' PageBGColor, '' GraphicsColor,isnull(NumProducts, 0) NumObjects, 20 PageSize, 0 QuantityDiscountID, '' Summary, SkinID, TemplateName
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


















    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_CreateMissingVariants]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_CreateMissingVariants]
    GO
    create proc [dbo].[aspdnsf_CreateMissingVariants] 
    WITH ENCRYPTION 
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



    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_SearchManufacturers]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_SearchManufacturers]
    GO
    create proc [dbo].[aspdnsf_SearchManufacturers]
        @SearchTerm nvarchar(3000),
        @ManufacturerID int = null
        WITH ENCRYPTION
    AS
    BEGIN
    SET NOCOUNT ON

        SET @SearchTerm = '%' + rtrim(ltrim(@SearchTerm)) + '%'
        SET @ManufacturerID = nullif(@ManufacturerID, 0)

        SELECT * 
        FROM dbo.Manufacturer with (NOLOCK) 
        WHERE deleted=0 
            and name like @SearchTerm 
            and ManufacturerID = coalesce(@ManufacturerID, ManufacturerID)

    END




    GO


    IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_SearchDistributors]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_SearchDistributors]
    go

    create proc [dbo].[aspdnsf_SearchDistributors]
        @SearchTerm nvarchar(3000),
        @DistributorID int = null
        WITH ENCRYPTION
    AS
    BEGIN
    SET NOCOUNT ON

        SET @SearchTerm = '%' + rtrim(ltrim(@SearchTerm)) + '%'
        SET @DistributorID = nullif(@DistributorID, 0)

        SELECT * 
        FROM dbo.Distributor with (NOLOCK) 
        WHERE deleted=0 
            and name like @SearchTerm 
            and DistributorID = coalesce(@DistributorID, DistributorID)

    END


    GO

    IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_SearchGenres]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_SearchGenres]
    go
    create proc [dbo].[aspdnsf_SearchGenres]
        @SearchTerm nvarchar(3000),
        @GenreID int = null
        WITH ENCRYPTION
    AS
    BEGIN
    SET NOCOUNT ON

        SET @SearchTerm = '%' + rtrim(ltrim(@SearchTerm)) + '%'
        SET @GenreID = nullif(@GenreID, 0)

        SELECT * 
        FROM dbo.Genre with (NOLOCK) 
        WHERE deleted=0 
            and name like @SearchTerm 
            and GenreID = coalesce(@GenreID, GenreID)

    END


    GO

    IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_SearchVectors]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_SearchVectors]
    go
    create proc [dbo].[aspdnsf_SearchVectors]
        @SearchTerm nvarchar(3000),
        @VectorID int = null
        WITH ENCRYPTION
    AS
    BEGIN
    SET NOCOUNT ON

        SET @SearchTerm = '%' + rtrim(ltrim(@SearchTerm)) + '%'
        SET @VectorID = nullif(@VectorID, 0)

        SELECT * 
        FROM dbo.vector with (NOLOCK) 
        WHERE deleted=0 
            and name like @SearchTerm 
            and VectorID = coalesce(@VectorID, VectorID)

    END


    GO





    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_DeleteAddress]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_DeleteAddress]
    GO
    CREATE proc [dbo].[aspdnsf_DeleteAddress]
        @AddressID int,
        @CustomerID int
        WITH ENCRYPTION
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


    if exists (select * from dbo.sysobjects where id= OBJECT_ID(N'dbo.aspdnsf_MarkOrderAsFraud') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE dbo.aspdnsf_MarkOrderAsFraud
    GO
    create proc dbo.aspdnsf_MarkOrderAsFraud
        @ordernum int,
        @fraudstate int
        WITH ENCRYPTION
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



    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_RemoveDuplicateAppConfigs]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_RemoveDuplicateAppConfigs]
    GO
    create proc [dbo].[aspdnsf_RemoveDuplicateAppConfigs] 
        WITH ENCRYPTION 
        as
    BEGIN
        delete from [dbo].appconfig where appconfigid in (select max(AppConfigID) as AppConfigID from AppConfig where name in (select name from appconfig group by name having count(name) > 1)  group by name)
    end

    GO

    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_StoreVersion]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_StoreVersion]
    GO
    create proc [dbo].[aspdnsf_StoreVersion] 
        WITH ENCRYPTION 
        as
    BEGIN
        select configvalue from dbo.appconfig with (nolock) where name='StoreVersion'
    end

    GO

    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_ShowDuplicateAppConfigs]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_ShowDuplicateAppConfigs]
    GO
    create proc [dbo].[aspdnsf_ShowDuplicateAppConfigs] 
        WITH ENCRYPTION 
        as
    BEGIN
        select * from dbo.appconfig with (nolock) where name in (select name from dbo.appconfig with (nolock) group by name having count(name) > 1) order by name
    end

    GO

    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_ResetAllProductVariantDefaults]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_ResetAllProductVariantDefaults]
    GO
    create proc [dbo].[aspdnsf_ResetAllProductVariantDefaults] 
        WITH ENCRYPTION 
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

    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_FindCircularReference]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_FindCircularReference]
    GO
    create proc [dbo].[aspdnsf_FindCircularReference]
        WITH ENCRYPTION
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


    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_GetSimpleObjectEntityList]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_GetSimpleObjectEntityList]
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
        WITH ENCRYPTION
        
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




    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_DropAllNonPrimaryIndexes]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_DropAllNonPrimaryIndexes]
    GO
    create proc [dbo].[aspdnsf_DropAllNonPrimaryIndexes] 
        WITH ENCRYPTION 
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

    -- ---------------------------------------------------------------------------
    -- aspdnsf_OrderAvgSummary
    -- ---------------------------------------------------------------------------
    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_OrderAvgSummary]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_OrderAvgSummary]
    go

    create proc [dbo].[aspdnsf_OrderAvgSummary]
    @transactionstate nvarchar(100)
        WITH ENCRYPTION
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
    go







    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_ListAllIndexes]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_ListAllIndexes]
    GO
    create proc [dbo].[aspdnsf_ListAllIndexes] 
        WITH ENCRYPTION 
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

    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_ListAllNonPrimaryIndexes]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_ListAllNonPrimaryIndexes]
    GO
    create proc [dbo].[aspdnsf_ListAllNonPrimaryIndexes] 
        WITH ENCRYPTION as
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

    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_GenerateCreatesForAllIndexes]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_GenerateCreatesForAllIndexes]
    GO
    create proc [dbo].[aspdnsf_GenerateCreatesForAllIndexes] 
	WITH ENCRYPTION 
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
    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_GetCartCategoryCountsByProduct]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_GetCartCategoryCountsByProduct]
    GO
    create proc dbo.aspdnsf_GetCartCategoryCountsByProduct
        @CustomerID int
        WITH ENCRYPTION
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

    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_DropColumnWithDefaultConstraint]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_DropColumnWithDefaultConstraint]
    GO
    create proc [dbo].[aspdnsf_DropColumnWithDefaultConstraint]
      @TableName varchar(100), 
      @ColumnName varchar(100) 
        WITH ENCRYPTION
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











    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_ReloadCart]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_ReloadCart]
    GO
    create proc [dbo].[aspdnsf_ReloadCart]
        @CartXML text
        WITH ENCRYPTION
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

     
     
     




     
     






    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_PageQuery]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_PageQuery]
    GO
    create procEDURE [dbo].[aspdnsf_PageQuery]
      @Select varchar(8000), 
      @OrderBy varchar(2000), 
      @PageNum int,
      @PageSize int,
      @StatsFirst int = 1
        WITH ENCRYPTION
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
    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_ImportProductPricing_XML]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_ImportProductPricing_XML]
    GO
    create proc [dbo].[aspdnsf_ImportProductPricing_XML]
        @pricing ntext
    WITH ENCRYPTION    
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

    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_GetRecentComments]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_GetRecentComments]
    GO
    create proc [dbo].[aspdnsf_GetRecentComments]
        @votingcustomer int,
        @pagesize int = 20,
        @pagenum int = 1,
        @sort tinyint = 0
        WITH ENCRYPTION
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
        WHERE r.HasComment <> 0 AND p.Deleted = 0 AND p.Published <> 0
        ORDER BY ' + case @sort when 0 then 'r.CreatedOn desc' when 1 then 'r.CreatedOn asc' when 2 then 'r.Rating desc, r.CreatedOn desc' when 3 then 'r.Rating asc, r.CreatedOn desc' end

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



    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_GetProductComments]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_GetProductComments]
    GO
    CREATE proc [dbo].[aspdnsf_GetProductComments]
        @ProductID int,
        @votingcustomer int,
        @pagesize int = 20,
        @pagenum int = 1,
        @sort tinyint = 0
        WITH ENCRYPTION
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
            left join [dbo].RatingCommentHelpfulness h with (nolock) on h.productid = r.ProductID and h.RatingCustomerID = r.CustomerID and h.VotingCustomerID = @votingcustomerid
            left join [dbo].Rating r2 with (nolock) on r2.CustomerID = @votingcustomerid and r.ProductID = r2.ProductID 
        WHERE r.HasComment <> 0 AND p.Deleted = 0 AND p.Published <> 0 and r.ProductID = @ProdID
        ORDER BY ' + case @sort 
                        when 1 then'r.FoundHelpful desc, r.CreatedOn desc' 
                        when 2 then'r.FoundHelpful asc, r.CreatedOn desc' 
                        when 3 then'r.CreatedOn desc' 
                        when 4 then'r.CreatedOn asc' 
                        when 5 then'r.Rating desc, r.CreatedOn desc' 
                        when 6 then'r.Rating asc, r.CreatedOn desc' 
                     end

        INSERT #tmp (RatingID, ProductID, CustomerID, Rating, Comments, FoundHelpful, FoundNotHelpful, CreatedOn, IsFilthy, ProductName, ProductSEName, ProductGuid, FirstName, LastName, RatingCustomerName, CommentHelpFul, MyRating)
        EXEC sp_executesql @cmd, N'@votingcustomerid int, @ProdID int', @votingcustomerid = @votingcustomer, @ProdID = @ProductID


        SET @totalcomments = @@rowcount

        SELECT @totalcomments totalcomments, ceiling(@totalcomments*1.0/@pagesize) pages

        SELECT * 
        FROM #tmp
        WHERE rownum >= @pagesize*(@pagenum-1)+1 and rownum <= @pagesize*(@pagenum)
        ORDER BY rownum


    END


    GO

    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_ProductStats]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_ProductStats]
    GO
    CREATE proc [dbo].[aspdnsf_ProductStats]
        @ProductID int
        WITH ENCRYPTION
    AS
    BEGIN
        SET NOCOUNT ON

        SELECT * 
        FROM dbo.product p with (nolock)
            left join (SELECT ProductID,count(rating) as NumRatings, sum(rating) as SumRatings, convert(decimal(4,3), avg(rating*1.0)) AvgRating
                       FROM dbo.Rating with (nolock)
                       WHERE ProductID = @ProductID
                       GROUP BY ProductID) ps on p.productid = ps.productid 
        WHERE p.ProductID = @ProductID

    END


    GO


   if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_BestSellers]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
    DROP PROCEDURE [dbo].[aspdnsf_BestSellers]
GO
create proc [dbo].[aspdnsf_BestSellers]
    @since   int = 180,  -- best sellers in the last "@since" number of days
    @return  int = 10,   -- returns the top "@returns" items
    @orderby tinyint = 1 -- 1 = order by count of sales for each product, 2 = order by total dollars sales for each product
 
WITH ENCRYPTION 
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
WHERE p.Deleted = 0 
    and p.Published = 1  
    and pv.Published = 1 
    and pv.Deleted = 0 
ORDER BY case @orderby when 1 then s.NumSales when 2 then s.NumDollars else s.NumSales end desc


SET @cmd = 'select top ' + convert(varchar(10), @return ) + ' * FROM #tmp order by id'

EXEC (@cmd)
END


GO



   if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_RecentAdditions]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
    DROP PROCEDURE [dbo].[aspdnsf_RecentAdditions]
GO
create proc [dbo].[aspdnsf_RecentAdditions]
    @since   int = 180,  -- products added in the last "@since" number of days
    @return  int = 10   -- returns the top "@returns" items
WITH ENCRYPTION 
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




			

                
    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_GetCustomersAlsoBoughtProducts]') AND type in (N'P', N'PC'))
            DROP PROCEDURE [dbo].[aspdnsf_GetCustomersAlsoBoughtProducts]
        GO

    CREATE PROC [dbo].[aspdnsf_GetCustomersAlsoBoughtProducts]  
            @NumProductToDisplay int,  
            @productid int,  
            @CustomerLevelID int,  
            @InvFilter int,  
            @affiliateID int  
        WITH ENCRYPTION
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



    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_CreateIndexes]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_CreateIndexes]
    GO
    create proc [dbo].[aspdnsf_CreateIndexes] 
        WITH ENCRYPTION as
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
    CREATE UNIQUE INDEX [UIX_AppConfig_Name] ON [AppConfig]([Name]);
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
    CREATE UNIQUE INDEX [UIX_ZipTaxRate_ZipCode_TaxClassID] ON [ZipTaxRate] (ZipCode,TaxClassID)
    CREATE UNIQUE INDEX [UIX_StateTaxRate_StateID_TaxClassID] ON [StateTaxRate] (StateID,TaxClassID)
    CREATE UNIQUE INDEX [UIX_CountryTaxRate_CountryID_TaxClassID] ON [CountryTaxRate] (CountryID,TaxClassID)
    CREATE UNIQUE INDEX [UIX_Vector_VectorGUID] ON [Vector]([VectorGUID]);
    CREATE INDEX [IX_Vector_Name] ON [Vector]([Name]) ;
    CREATE INDEX [IX_Vector_DisplayOrder_Name] ON [Vector]([DisplayOrder],[Name]) ;
    END

    GO





    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_DropAndRecreateAllNonPrimaryIndexes]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_DropAndRecreateAllNonPrimaryIndexes]
    GO
    create proc [dbo].[aspdnsf_DropAndRecreateAllNonPrimaryIndexes] 
        WITH ENCRYPTION as
    BEGIN
        exec dbo.aspdnsf_DropAllNonPrimaryIndexes
        exec dbo.aspdnsf_CreateIndexes
    END

    GO





    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_OrderSummary]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_OrderSummary]
    GO
    CREATE proc [dbo].[aspdnsf_OrderSummary]
        @transactionstate nvarchar(100)
        WITH ENCRYPTION
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


    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_DelFeed]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_DelFeed]
    GO
    create proc [dbo].[aspdnsf_DelFeed]
        @FeedID int
        WITH ENCRYPTION
    AS
    BEGIN
    SET NOCOUNT ON 

        DELETE dbo.Feed WHERE FeedID = @FeedID

    END


    GO
    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_UpdFeed]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_UpdFeed]

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
        WITH ENCRYPTION
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
    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_CreateFeed]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_CreateFeed]
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
        WITH ENCRYPTION
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




    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_GetFeed]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_GetFeed]
    GO
    create proc [dbo].[aspdnsf_GetFeed]
        @FeedID int = null
        WITH ENCRYPTION
    AS
    BEGIN
    SET NOCOUNT ON 

    SELECT FeedID, FeedGUID, Name, DisplayOrder, XmlPackage, CanAutoFTP, FTPUsername, FTPPassword, FTPServer, FTPPort, FTPFilename, isnull(ExtensionData, '') ExtensionData, CreatedOn
    FROM dbo.Feed
    WHERE FeedID = COALESCE(@FeedID, FeedID)

    END

    GO






    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_UpdateCartKitPrice]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_UpdateCartKitPrice]
    GO
    create proc dbo.aspdnsf_UpdateCartKitPrice
        @ShoppingCartRecId int,
        @CustomerLevelID int = 0
        WITH ENCRYPTION
    AS
    BEGIN
        SET NOCOUNT ON
        DECLARE @LevelDiscountsApplyToExtendedPrices tinyint, @LevelDiscountPercent money, @ProductPrice money

        SELECT @LevelDiscountsApplyToExtendedPrices = LevelDiscountsApplyToExtendedPrices, @LevelDiscountPercent = LevelDiscountPercent  
        FROM customerlevel
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


    if exists (select * from [dbo].sysobjects where id= OBJECT_ID(N'[dbo].[aspdnsf_ImportOrderShipment_XML]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_ImportOrderShipment_XML]
    GO

    create PROC [dbo].[aspdnsf_ImportOrderShipment_XML]  
     @xmlorder ntext,
     @carrierName ntext  
        WITH ENCRYPTION
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








    if exists (select * from [dbo].sysobjects where id = OBJECT_ID(N'[dbo].[aspdnsf_GetProductsEntity]') and OBJECTPROPERTY(id,N'IsProcedure') = 1)
        DROP PROCEDURE [dbo].[aspdnsf_GetProductsEntity]
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
        WITH ENCRYPTION
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









    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_GetCustomerByID]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_GetCustomerByID]
    go
    create proc [dbo].[aspdnsf_GetCustomerByID]
        @CustomerID int
        WITH ENCRYPTION
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
                @LastActivity LastActivity
        FROM dbo.Customer c with (nolock) left join dbo.CustomerLevel cl with (nolock) on c.CustomerLevelID = cl.CustomerLevelID
        WHERE c.Deleted=0 and c.CustomerID = @CustomerID


    END


    GO



    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_GetCustomerByGUID]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_GetCustomerByGUID]
    go
    create proc [dbo].[aspdnsf_GetCustomerByGUID]
        @CustomerGUID uniqueidentifier
        WITH ENCRYPTION
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
                @CustomerSessionID CustomerSessionID,
                @LastActivity LastActivity
        FROM dbo.Customer c with (nolock) left join dbo.CustomerLevel cl with (nolock) on c.CustomerLevelID = cl.CustomerLevelID
        WHERE c.Deleted=0 and c.CustomerGUID = @CustomerGUID


    END


    GO













    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_GetCustomerByEmail]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_GetCustomerByEmail]
    go
    create proc [dbo].[aspdnsf_GetCustomerByEmail]
        @Email nvarchar(100)
        WITH ENCRYPTION
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
                @CustomerSessionID CustomerSessionID,
                @LastActivity LastActivity
        FROM dbo.Customer c with (nolock) left join dbo.CustomerLevel cl with (nolock) on c.CustomerLevelID = cl.CustomerLevelID
        WHERE c.Deleted=0 and c.Email = @Email


    END



    GO











    if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_updCustomer]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
        drop procedure [dbo].[aspdnsf_updCustomer]
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
        @RequestedPaymentMethod  nvarchar(100) = null
        WITH ENCRYPTION
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
        RequestedPaymentMethod = COALESCE(@RequestedPaymentMethod, RequestedPaymentMethod)
    WHERE CustomerID = @CustomerID

    IF @IsAdminCust > 0 and @OldPwd <> @Password
        INSERT dbo.PasswordLog (CustomerID, OldPwd, SaltKey, ChangeDt)
        VALUES (@CustomerID, @OldPwd, @OldSaltKey, getdate())
    GO





    if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_updCustomerByEmail]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
        drop procedure [dbo].[aspdnsf_updCustomerByEmail]
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
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 

    DECLARE @CustomerID int, @OldPwd nvarchar(100), @IsAdminCust tinyint, @OldSaltKey int

    SELECT @CustomerID = CustomerID , @OldPwd = Password, @IsAdminCust = IsAdmin, @OldSaltKey = Saltkey FROM dbo.Customer WHERE Email = @Email 


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







    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_SessionGetByCustomerID]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_SessionGetByCustomerID]
    GO
    create proc [dbo].[aspdnsf_SessionGetByCustomerID]
        @CustomerID int
        WITH ENCRYPTION
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



    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_SessionGetByGUID]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_SessionGetByGUID]
    GO
    create proc [dbo].[aspdnsf_SessionGetByGUID]
        @CustomerSessionGUID uniqueidentifier
        WITH ENCRYPTION
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
    FROM dbo.Customersession
    WHERE CustomerSessionGUID = @CustomerSessionGUID and LoggedOut is null and LastActivity > dateadd(mi, -@intSessionTimeOut, getdate())

    go



    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_SessionGetByID]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_SessionGetByID]
    GO
    create proc [dbo].[aspdnsf_SessionGetByID]
        @CustomerSessionID int
        WITH ENCRYPTION
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
    FROM dbo.Customersession
    WHERE CustomerSessionID = @CustomerSessionID and LoggedOut is null and LastActivity > dateadd(mi, -@intSessionTimeOut, getdate())



    GO



    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_SessionInsert]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_SessionInsert]
    GO
    create proc [dbo].[aspdnsf_SessionInsert]
        @CustomerID int,
        @SessionValue ntext,
        @ipaddr varchar(15),
        @CustomerSessionID int OUTPUT
        WITH ENCRYPTION
    AS
    SET NOCOUNT ON 

    DECLARE @CustomerSessionGUID uniqueidentifier

    set @CustomerSessionGUID = newid()
    insert dbo.Customersession(CustomerID, SessionName, SessionValue, CreatedOn, ipaddr, LastActivity, CustomerSessionGUID)
    values (@CustomerID, '', isnull(@SessionValue, ''), getdate(), @ipaddr, getdate(), @CustomerSessionGUID)

    set @CustomerSessionID = @@identity

    DELETE dbo.Customersession WHERE CustomerID = @CustomerID and CustomersessionID <> @CustomerSessionID 


    GO


    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_SessionAge]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_SessionAge]
    GO
    create proc [dbo].[aspdnsf_SessionAge]
        @CustomerID int = null
        WITH ENCRYPTION
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



    IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_SessionUpdate]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_SessionUpdate]
    GO
    CREATE proc [dbo].[aspdnsf_SessionUpdate]
        @CustomerSessionID int,
        @SessionName nvarchar(100),
        @SessionValue ntext,
        @ExpiresOn datetime,
        @LoggedOut datetime
        WITH ENCRYPTION
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
    FROM dbo.Customersession
    WHERE CustomerSessionID = @CustomerSessionID 


    GO







/****************************************   Data    *******************************************/

    -- UPS shipping abbreviation mod
    update State set Abbreviation='NL' where Name='Newfoundland and Labrador';
    go





    if not exists(select * from dbo.AppConfig where name = 'CENTRALPAYMENTS_AssociateName')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'CENTRALPAYMENTS_AssociateName','PAYMENTS','Your CentralPayments assigned associate name','FCTEST');
    if not exists(select * from dbo.AppConfig where name = 'CENTRALPAYMENTS_AssociatePassword')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'CENTRALPAYMENTS_AssociatePassword','PAYMENTS','Your CentralPayments assigned associate password','FCTEST');
    if not exists(select * from dbo.AppConfig where name = 'QBMERCHANTSERVICES_ApplicationLogin')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QBMERCHANTSERVICES_ApplicationLogin','PAYMENTS','do not change this value','AspDotNetStorefront');
    if not exists(select * from dbo.AppConfig where name = 'QBMERCHANTSERVICES_ApplicationID')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QBMERCHANTSERVICES_ApplicationID','PAYMENTS','do not change this value','');
    if not exists(select * from dbo.AppConfig where name = 'QBMERCHANTSERVICES_ConnectionTicket')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QBMERCHANTSERVICES_ConnectionTicket','PAYMENTS','Your connection ticket assigned by QuickBooks. See Quick Books Merchant Services for instructions on how to obtain your connection ticket.','');
    if not exists(select * from dbo.AppConfig where name = 'QBMERCHANTSERVICES_SessionTicket')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QBMERCHANTSERVICES_SessionTicket','PAYMENTS','Your session ticket assigned by QuickBooks. See Quick Books Merchant Services for instructions on how to obtain your session ticket.','');
    if not exists(select * from dbo.AppConfig where name = 'DisallowShippingToPOBoxes')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DisallowShippingToPOBoxes','SHIPPING','If you do not want to allow, or cannot allow (e.g. using FEDEX) shipping to PO Boxes, set this flag to true, and the store will TRY to prevent users from [dbo].entering PO Box shipping addresses. It is NOT 100% guaranteed. Users enter all types of crap!','false');

    if not exists(select * from dbo.AppConfig where name = 'TRANSACTIONCENTRAL_RegKey')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'TRANSACTIONCENTRAL_RegKey','GATEWAY','Your RegKey. Provided by Transaction Central (primerchants.com).','');
    go

    if not exists (select * from dbo.AppConfig where name = 'Innova.XHTML')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Innova.XHTML','DISPLAY','If true, the admin site innova html editor fields will use XHTML format and validation. If false, they will use straight HTML format.','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'HideProductsWithLessThanThisInventoryLevel')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'HideProductsWithLessThanThisInventoryLevel','DISPLAY','if not-zero, the site will filter out products that have less than the inventory # in stock that you specify here. Leave to 0 to disable this filtering. For products with multiple variants, and sizes & colors, etc, the SUM of those inventory items is used to determine if to filter the product. So the sum of all variant inventory must be less than this number before it will be filtered out.','0');
    go

    if not exists(select * from dbo.AppConfig where name = 'FilterOutShippingMethodsThatHave0Cost')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'FilterOutShippingMethodsThatHave0Cost','SHIPPING','if you want to filter out shipping methods that result in $0 costs to the customer, you can set this flag to true. Otherwise, all shipping methods are returned and displayed. This appconfig should almost always be false.','false');
    go
    if not exists(select * from dbo.AppConfig where name = 'AutoSelectFirstSizeColorOption')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'AutoSelectFirstSizeColorOption','DISPLAY','if true, the first "option" in the size or color select lists in the add to cart forms will be auto-selected in the add to cart forms.','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'XmlPackage.DefaultDistributorNotification')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'XmlPackage.DefaultDistributorNotification','XMLPACKAGE','The XmlPackage used by default to send new order notifications to distributors. You can override the XmlPackage to be used on each distributor however in the admin site','notification.distributor.xml.config');
    go

    if not exists(select * from dbo.AppConfig where name = 'XmlPackage.LostPassword')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'XmlPackage.LostPassword','XMLPACKAGE','The XmlPackage used to generate lost password messages back to customers','notification.lostpassword.xml.config');
    go
    if not exists(select * from dbo.AppConfig where name = 'XmlPackage.OrderReceipt')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'XmlPackage.OrderReceipt','XMLPACKAGE','The XmlPackage used to generate order receipts for customers','notification.receipt.xml.config');
    go
    if not exists(select * from dbo.AppConfig where name = 'XmlPackage.OrderAsXm')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'XmlPackage.OrderAsXml','XMLPACKAGE','The XmlPackage used to convert an order into Xml format (for administrative purposes, for export purposes, etc)','notification.orderasxml.xml.config');
    go
    if not exists(select * from dbo.AppConfig where name = 'XmlPackage.OrderShipped')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'XmlPackage.OrderShipped','XMLPACKAGE','The XmlPackage used to create the "your order has shipped" e-mail notifications to customers','notification.shipped.xml.config');
    go
    if not exists(select * from dbo.AppConfig where name = 'XmlPackage.NewOrderAdminNotification')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'XmlPackage.NewOrderAdminNotification','XMLPACKAGE','The XmlPackage used when sending an e-mail to the store administrator when a new order is received','notification.adminneworder.xml.config');
    go
    if not exists(select * from dbo.AppConfig where name = 'XmlPackage.NewOrderAdminSMSNotification')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'XmlPackage.NewOrderAdminSMSNotification','XMLPACKAGE','The XmlPackage used when sending a SMS cell phone message to the store administrator when a new order is received','notification.smsneworder.xml.config');
    go
    if not exists(select * from dbo.AppConfig where name = 'XmlPackage.AffiliateSignupNotification')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'XmlPackage.AffiliateSignupNotification','XMLPACKAGE','The XmlPackage used to create the e-mail message to be sent to new affiliates when they sign-up.','notification.affiliatesignup.xml.config');
    go
    if not exists(select * from dbo.AppConfig where name = 'XmlPackage.OrderFinalization')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'XmlPackage.OrderFinalization','XMLPACKAGE','The XmlPackage used to create the order finalization instructions during checkout','finalizationdata.xml.config');
    go
    if not exists(select * from dbo.AppConfig where name = 'RequireOver13Checked')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'RequireOver13Checked','SECURITY','if true, on creating an account, the users must check a checkbox indicating they are over 13. this may be required for federal regulation compliance.','true');
    go

    if not exists(select * from dbo.AppConfig where name = 'Localization.CurrencyFeedUrl')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Localization.CurrencyFeedUrl','LOCALIZATION','the fully specified url that you will be using to get Currency Exchange Rate Data. See also AppConfig:Localization.CurrencyFeedXmlPackage','http://www.ecb.int/stats/eurofxref/eurofxref-daily.xml');
    go
    if not exists(select * from dbo.AppConfig where name = 'Localization.CurrencyFeedXmlPackage')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Localization.CurrencyFeedXmlPackage','LOCALIZATION','The xmlpackage that you want to use to provide the currency exchange rate data conversion. This package usually must work hand in hand with the currency exchange rate data provider, to convert their rate data into our predefined xml format. See also AppConfig:Localization.CurrencyFeedUrl','feed.exchangerates-eur.central.bank.xml.config');
    go
    if not exists(select * from dbo.AppConfig where name = 'Localization.CurrencyFeedBaseRateCurrencyCode')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Localization.CurrencyFeedBaseRateCurrencyCode','LOCALIZATION','You MUST set this appconfig to match the BASE currency code that your currency feed is returning exchange rates relative to, e.g. USD, EUR, etc.','EUR');
    go
    if not exists(select * from dbo.AppConfig where name = 'Localization.CurrencyCacheMinutes')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Localization.CurrencyCacheMinutes','LOCALIZATION','The time that a currency rate table is cached. The site will call the live currencyserver specified in AppConfig:Localization.CurrencyFeedUrl parameter every time this cache period expires','60');
    go

    if not exists(select * from dbo.AppConfig where name = 'CENTRALPAYMENTS_AssociateName')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'CENTRALPAYMENTS_AssociateName','PAYMENTS','Your CentralPayments assigned associate name','FCTEST');
    go
    if not exists(select * from dbo.AppConfig where name = 'CENTRALPAYMENTS_AssociatePassword')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'CENTRALPAYMENTS_AssociatePassword','PAYMENTS','Your CentralPayments assigned associate password','FCTEST');
    go

    if not exists(select * from dbo.AppConfig where name = 'QBMERCHANTSERVICES_ApplicationLogin')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QBMERCHANTSERVICES_ApplicationLogin','PAYMENTS','Your Application login that you obtained from QuickBooks. See manual for instructions.','');
    go
    if not exists(select * from dbo.AppConfig where name = 'QBMERCHANTSERVICES_ApplicationID')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QBMERCHANTSERVICES_ApplicationID','PAYMENTS','Your Application ID that you obtained from QuickBooks. See manual for instructions.','');
    go
    if not exists(select * from dbo.AppConfig where name = 'QBMERCHANTSERVICES_ApplicationVersion')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QBMERCHANTSERVICES_ApplicationVersion','PAYMENTS','do not change this value','1.0');
    go
    if not exists(select * from dbo.AppConfig where name = 'QBMERCHANTSERVICES_InstallID')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QBMERCHANTSERVICES_InstallID','PAYMENTS','','');
    go
    if not exists(select * from dbo.AppConfig where name = 'QBMERCHANTSERVICES_ConnectionTicket')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QBMERCHANTSERVICES_ConnectionTicket','PAYMENTS','Your connection ticket assigned by QuickBooks. See Quick Books Merchant Services and our manual page for instructions on how to obtain your connection ticket.','');
    go
    if not exists(select * from dbo.AppConfig where name = 'QBMERCHANTSERVICES_Language')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QBMERCHANTSERVICES_Language','PAYMENTS','Your connection ticket assigned by QuickBooks. See Quick Books Merchant Services for instructions on how to obtain your connection ticket.','English');
    go

    if not exists(select * from dbo.AppConfig where name = 'Default_CategoryPageSize')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Default_CategoryPageSize','SITEDISPLAY','The default pagesize for a category, when it is first added to the db. After adding, you can edit in the admin site. Set to 0 for no paging (i.e. all products are listed). This value MAY be used by the XmlPackage displaying this page.','20');
    go
    if not exists(select * from dbo.AppConfig where name = 'Default_SectionPageSize')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Default_SectionPageSize','SITEDISPLAY','The default pagesize for a section, when it is first added to the db. After adding, you can edit in the admin site. Set to 0 for no paging (i.e. all products are listed). This value MAY be used by the XmlPackage displaying this page.','20');
    go
    if not exists(select * from dbo.AppConfig where name = 'Default_LibraryPageSize')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Default_LibraryPageSize','SITEDISPLAY','The default pagesize for a library, when it is first added to the db. After adding, you can edit in the admin site. Set to 0 for no paging (i.e. all products are listed). This value MAY be used by the XmlPackage displaying this page.','20');
    go
    if not exists(select * from dbo.AppConfig where name = 'Default_ManufacturerPageSize')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Default_ManufacturerPageSize','SITEDISPLAY','The default pagesize for a manufacturer, when it is first added to the db. After adding, you can edit in the admin site. Set to 0 for no paging (i.e. all products are listed). This value MAY be used by the XmlPackage displaying this page.','20');
    go
    if not exists(select * from dbo.AppConfig where name = 'Default_DistributorPageSize')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Default_DistributorPageSize','SITEDISPLAY','The default pagesize for a distributor, when it is first added to the db. After adding, you can edit in the admin site. Set to 0 for no paging (i.e. all products are listed). This value MAY be used by the XmlPackage displaying this page.','20');
    go
    if not exists(select * from dbo.AppConfig where name = 'Default_ProductPageSize')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Default_ProductPageSize','SITEDISPLAY','The default pagesize for a product, when it is first added to the db. After adding, you can edit in the admin site. Set to 0 for no paging (i.e. all variants are listed). This value MAY be used by the XmlPackage displaying this page.','20');
    go
    if not exists(select * from dbo.AppConfig where name = 'Default_DocumentPageSize')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Default_DocumentPageSize','SITEDISPLAY','The default pagesize for a document, when it is first added to the db. After adding, you can edit in the admin site. Set to 0 for no paging (i.e. all documents are listed). This value MAY be used by the XmlPackage displaying this page.','20');
    go

    if not exists(select * from dbo.AppConfig where name = 'Default_CategoryColWidth')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Default_CategoryColWidth','SITEDISPLAY','The default ColWidth for a category, when it is first added to the db. After adding, you can edit in the admin site. Set to 0 for no paging (i.e. all products are listed). This value MAY be used by the XmlPackage displaying this page.','4');
    go
    if not exists(select * from dbo.AppConfig where name = 'Default_SectionColWidth')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Default_SectionColWidth','SITEDISPLAY','The default ColWidth for a section, when it is first added to the db. After adding, you can edit in the admin site. Set to 0 for no paging (i.e. all products are listed). This value MAY be used by the XmlPackage displaying this page.','4');
    go
    if not exists(select * from dbo.AppConfig where name = 'Default_LibraryColWidth')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Default_LibraryColWidth','SITEDISPLAY','The default ColWidth for a library, when it is first added to the db. After adding, you can edit in the admin site. Set to 0 for no paging (i.e. all products are listed). This value MAY be used by the XmlPackage displaying this page.','4');
    go
    if not exists(select * from dbo.AppConfig where name = 'Default_ManufacturerColWidth')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Default_ManufacturerColWidth','SITEDISPLAY','The default ColWidth for a manufacturer, when it is first added to the db. After adding, you can edit in the admin site. Set to 0 for no paging (i.e. all products are listed). This value MAY be used by the XmlPackage displaying this page.','4');
    go
    if not exists(select * from dbo.AppConfig where name = 'Default_DistributorColWidth')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Default_DistributorColWidth','SITEDISPLAY','The default ColWidth for a distributor, when it is first added to the db. After adding, you can edit in the admin site. Set to 0 for no paging (i.e. all products are listed). This value MAY be used by the XmlPackage displaying this page.','4');
    go
    if not exists(select * from dbo.AppConfig where name = 'Default_ProductColWidth')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Default_ProductColWidth','SITEDISPLAY','The default ColWidth for a product, when it is first added to the db. After adding, you can edit in the admin site. Set to 0 for no paging (i.e. all variants are listed). This value MAY be used by the XmlPackage displaying this page.','4');
    go
    if not exists(select * from dbo.AppConfig where name = 'Default_DocumentColWidth')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Default_DocumentColWidth','SITEDISPLAY','The default ColWidth for a document, when it is first added to the db. After adding, you can edit in the admin site. Set to 0 for no paging (i.e. all documents are listed). This value MAY be used by the XmlPackage displaying this page.','4');
    go
    if not exists(select * from dbo.AppConfig where name = 'DisallowShippingToPOBoxes')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DisallowShippingToPOBoxes','SHIPPING','If you do not want to allow, or cannot allow (e.g. using FEDEX) shipping to PO Boxes, set this flag to true, and the store will TRY to prevent users from entering PO Box shipping addresses. It is NOT 100% guaranteed. Users enter all types of crap!','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'UseMappingShipToPayment')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'UseMappingShipToPayment','SHIPPING','if true, you can via the admin site map which shipping methods are allowed for the payment methods. This is an undocumented and unsupported feature.','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'RelatedProductsGridColWidth')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'RelatedProductsGridColWidth','DISPLAY','if using grid layout for related products, this is the # of cols in the grid','4');

    if not exists (select * FROM dbo.appconfig where name = 'UpsellProductsGridColWidth')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'UpsellProductsGridColWidth','DISPLAY','if using grid layout for upsell products, this is the # of cols in the grid','4');

    if not exists(select * from dbo.AppConfig where name = 'GiftCard.PhysicalProductTypeIDs')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'GiftCard.PhysicalProductTypeIDs','GIFTCARD','A comma delimited list of the ProductTypeIDs that identify a Physical Gift Card. Physical Gift Cards have their Serial numbers assigned when they are shipped to the customer.','100');

    if not exists(select * from dbo.AppConfig where name = 'GiftCard.EmailProductTypeIDs')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'GiftCard.EmailProductTypeIDs','GIFTCARD','A comma delimited list of the ProductTypeIDs that identify an Email Gift Card. Email Gift Cards are emailed to the recipient and have their Serial numbers assigned automatically.','101');

    if not exists(select * from dbo.AppConfig where name = 'GiftCard.CertificateProductTypeIDs')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'GiftCard.CertificateProductTypeIDs','GIFTCARD','A comma delimited list of the ProductTypeIDs that identify an Certificate Gift Card. Certificate Gift Cards are printed by the customer so they may be given by the customer to the recipient and will have their Serial numbers assigned automatically.','102');

    if not exists(select * from dbo.AppConfig where name = 'SecurityCodeRequiredOnCreateAccount')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'SecurityCodeRequiredOnCreateAccount','SECURITY','If true, turing number security fields are added to the create account page (not checkout mode), to prevent automated attacks. Turing fields are also ONLY used on the live server, not the dev server, so make sure your AppConfig:LiveServer is set to be yourdomain.com also','false');

    if not exists(select * from dbo.AppConfig where name = 'SecurityCodeRequiredOnCreateAccountDuringCheckout')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'SecurityCodeRequiredOnCreateAccountDuringCheckout','SECURITY','If true, turing number security fields are added to the create account page during checkout, to prevent automated attacks. Turing fields are also ONLY used on the live server, not the dev server, so make sure your AppConfig:LiveServer is set to be yourdomain.com also','false');

    if not exists(select * from dbo.AppConfig where name = 'SubscriptionExtensionOccursFromOrderDate')
        insert [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'SubscriptionExtensionOccursFromOrderDate','MISC','if false, the order extends a customers subscription from the PRIOR expiration date, if true, it sets the expiration date to the order date + subscription interval in the order.','false');




    -- v6.2.1 added:
    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.ShowOnCartPage')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'GoogleCheckout.ShowOnCartPage','GOOGLE CHECKOUT','This enables or disables Google Checkout on the Shopping Cart page. If true, the Google Checkout button will be shown (assuming the shopping cart is not empty). If false, the Google Checkout button will not be shown, and Google Checkout cannot be used.','true');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.UseSandbox')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'GoogleCheckout.UseSandbox','GOOGLE CHECKOUT','This enables use of the Google Sandbox for test transactions. If true, transactions will be sent to sandbox.google.com for testing. If false, transactions will be "live", and will be processed at checkout.google.com.','true');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.DefaultShippingMarkup')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'GoogleCheckout.DefaultShippingMarkup','GOOGLE CHECKOUT','Google Checkout requires "default shipping" options when the cart is initially loaded. This is the multiplier applied to the base shipping rates, to calculate the "default shipping" rates.','2.0');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.MerchantId')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'GoogleCheckout.MerchantId','GOOGLE CHECKOUT','From the Settings>Intergration page at checkout.google.com/sell','');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.MerchantKey')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'GoogleCheckout.MerchantKey','GOOGLE CHECKOUT','From the Settings>Intergration page at checkout.google.com/sell','');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.SandboxMerchantId')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'GoogleCheckout.SandboxMerchantId','GOOGLE CHECKOUT','From the Settings>Intergration page at sandbox.google.com/sell','');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.SandboxMerchantKey')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'GoogleCheckout.SandboxMerchantKey','GOOGLE CHECKOUT','From the Settings>Intergration page at sandbox.google.com/sell','');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.LogMessages')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'GoogleCheckout.LogMessages','GOOGLE CHECKOUT','This true/false setting enables diagnostic troubleshooting messages to be logged to a text file. As messages are being transmitted between your site and Google Checkout, they are also written to a text file for later analysis.','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.LogFileName')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'GoogleCheckout.LogFileName','GOOGLE CHECKOUT','The filename of the log file, relative to the root of your website. Enter a relative pathname here. Absolute pathnames won''t work.','GoogleCheckout.txt');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.BaseUrl')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'GoogleCheckout.BaseUrl','GOOGLE CHECKOUT','Don''t change this value. It''s the URL that your website uses to communicate with Google Checkout.','https://checkout.google.com/cws/v2/Merchant/');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.DiagnosticsOnly')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'GoogleCheckout.DiagnosticsOnly','GOOGLE CHECKOUT','Don''t set this value to true, as it will cause Google Checkout to stop working. If it''s set to true, all communications between your site and Google Checkout will be done in diagnostic-mode, which is used to check for XML syntax errors.','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.SendStoreReceipt')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.SendStoreReceipt','GOOGLE CHECKOUT','This forces the store to send the store recpeit in addition to the receipt sent out by GoogleCheckout.  In some cases the store receipt has needed information on it that the Google Checkout receipt doesn''t.','false');
    go




    if not exists(select * from dbo.AppConfig where name = 'PayPalExpress.PageStyle')
        INSERT dbo.AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'PayPalExpress.PageStyle','GATEWAYS','Sets the Custom Payment Page Style for payment pages associated with this button/link. PageStyle corresponds to the HTML variable page_style for customizing payment pages. The value is the same as the Page Style Name you chose when adding or editing the page style from the Profile subtab of the My Account tab of your PayPal account.','');
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPalExpress.HeaderImage')
        INSERT dbo.AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'PayPalExpress.HeaderImage','GATEWAYS','A URL for the image you want to appear at the top left of the payment page. The image has a maximum size of 750 pixels wide by 90 pixels high. PayPal recommends that you provide an image that is stored on a secure (https) server.','');
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPalExpress.HeaderBackColor')
        INSERT dbo.AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'PayPalExpress.HeaderBackColor','GATEWAYS','Sets the background color for the header of the payment page. Character length and limitation: Six character HTML hexadecimal color code in ASCII','');
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPalExpress.HeaderBorderColor')
        INSERT dbo.AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'PayPalExpress.HeaderBorderColor','GATEWAYS','Sets the border color around the header of the payment page. The border is a 2-pixel perimeter around the header space, which is 750 pixels wide by 90 pixels high. Character length and limitations: Six character HTML hexadecimal color code in ASCII','');
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPalExpress.PayFlowColor')
        INSERT dbo.AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'PayPalExpress.PayFlowColor','GATEWAYS','Sets the background color for the payment page. Character length and limitation: Six character HTML hexadecimal color code in ASCII','');
    go

    if not exists (select * from dbo.AppConfig where name = 'SearchAdv_ShowDistributor')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'SearchAdv_ShowDistributor','SEARCH','if true, then a distributor filter will be shown in the advanced search page','true');
    go

    if not exists (select * from dbo.AppConfig where name = 'Search_ShowDistributorsInResults')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Search_ShowDistributorsInResults','SEARCH','if true, then a distributor matches will be shown in the advanced search results','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'Skipjack.LiveServer')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Skipjack.LiveServer','GATEWAY','Your SkipJack live production transaction server post URL.','https://www.skipjackic.com/scripts/evolvcc.dll?AuthorizeAPI');
    go
    if not exists(select * from dbo.AppConfig where name = 'Skipjack.TestServer')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Skipjack.TestServer','GATEWAY','Your SkipJack test transaction server post URL.','https://developer.skipjackic.com/scripts/evolvcc.dll?AuthorizeAPI');
    go
    if not exists(select * from dbo.AppConfig where name = 'Skipjack.LiveSerialNumber')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Skipjack.LiveSerialNumber','GATEWAY','Your SkipJack assigned live account html serial number. You get this from SkipJack','');
    go
    if not exists(select * from dbo.AppConfig where name = 'Skipjack.DeveloperSerialNumber')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Skipjack.DeveloperSerialNumber','GATEWAY','Your SkipJack assigned developer serial number. You get this from SkipJack','');
    go
    if not exists(select * from dbo.AppConfig where name = 'Skipjack.LiveChangeURL')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Skipjack.LiveChangeURL','GATEWAY','Your SkipJack live production change transaction server post URL.','https://www.skipjackic.com/scripts/evolvcc.dll?SJAPI_TransactionChangeStatusRequest');
    go
    if not exists(select * from dbo.AppConfig where name = 'Skipjack.TestChangeURL')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Skipjack.TestChangeURL','GATEWAY','Your SkipJack test change transaction server post URL.','https://developer.skipjackic.com/scripts/evolvcc.dll?SJAPI_TransactionChangeStatusRequest');
    go
    if not exists(select * from dbo.AppConfig where name = 'Skipjack.TestSerialNumber')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Skipjack.TestSerialNumber','GATEWAY','Your SkipJack assigned test account html serial number. You get this from SkipJack','');
    go
    if not exists(select * from dbo.AppConfig where name = 'Skipjack.ForceSettlement')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Skipjack.ForceSettlement','GATEWAY','Used for SkipJack transaction changes for SETTLE and CREDIT to include a ForceSettlement option. Typically this is False.','False');
    go


    if not exists (select * from dbo.AppConfig where name = 'ShowPayPalExpressCheckoutOnCartPage')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ShowPayPalExpressCheckoutOnCartPage','GATEWAYS','If you want the PayPal Express Checkout button on your cart page, which we think is very confusing, set this to true. By default, we set this to false, so they can select the checkout method on our payments page like all other payment methods. The icon will not be shown if the cart is empty also.','false');
    go

    if not exists (select * from dbo.AppConfig where name = 'XmlPackage.EmailGiftCardNotification')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'XmlPackage.EmailGiftCardNotification','XMLPACKAGE','The package used to create body of the email sent when some one purchases an email gift card.','notification.emailgiftcard.xml.config');
    go

    if not exists (select * from dbo.AppConfig where name = 'PayPalPro.Signature')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'PayPalPro.Signature','GATEWAY','You can use this API Signature instead of the PayPal Pro Cert files. this approach is much easier.','');
    go

    if not exists (select * from dbo.AppConfig where name = 'MaxMind.Enabled')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'MaxMind.Enabled','GATEWAYS','If true, the MaxMind fraud prevention score will be checked before a credit card is sent to the gateway. If the returned FraudScore exceeds AppLogic.MaxMind.FailScoreThreshold, the order will be failed. See MaxMind.com for more documentation.','false');
    go

    if not exists (select * from dbo.AppConfig where name = 'MaxMind.LicenseKey')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'MaxMind.LicenseKey','GATEWAYS','Your MaxMind license key. Issued by MaxMind. Consult MaxMind.com for documentation','');
    go

    if not exists (select * from dbo.AppConfig where name = 'MaxMind.FailScoreThreshold')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'MaxMind.FailScoreThreshold','GATEWAYS','Threshold over which to fail orders. 0.0 is lowest risk. 10.0 is highest risk. By default, this setting (of 10.0) will NOT FAIL any order. You will have to set this threshold to your own liking for your own business. Every store will probably use different thresholds due to the nature of their business. Consult MaxMind.com for documentation. ','10.0');
    go

    if not exists (select * from dbo.AppConfig where name = 'MaxMind.DelayDownloadThreshold')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'MaxMind.DelayDownloadThreshold','GATEWAYS','Threshold over which to force delayed downloads on orders, regardless of the setting of AppConfig:DelayedDownloads is set to. 0.0 is lowest risk. 10.0 is highest risk. See AppConfig:MaxMind.ScoreThreshold also.','2.5');
    go

    if not exists (select * from dbo.AppConfig where name = 'MaxMind.DelayDropShipThreshold')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'MaxMind.DelayDropShipThreshold','GATEWAYS','Threshold over which to force delayed downloads on orders, regardless of the setting of AppConfig:DelayedDropShipNotifications is set to. 0.0 is lowest risk. 10.0 is highest risk. See AppConfig:MaxMind.ScoreThreshold also.','2.5');
    go


    if not exists (select * from dbo.AppConfig where name = 'SendWelcomeEmail')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'SendWelcomeEmail','MISC','Set to true if you want new customers to get a welcoming email.  The email message content is controled by the XmlPackage specified in the XmlPackage.WelcomeEmail Appconfig parameter.  The email subject is stored in the StringResource createaccount.aspx.79','false');
    go

    if not exists (select * from dbo.AppConfig where name = 'XmlPackage.WelcomeEmail')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'XmlPackage.WelcomeEmail','XMLPACKAGE','Used for the contents of the welcome email that newly registered customers receive.','notification.newmemberwelcome.xml.config');
    go

    if not exists (select * from dbo.AppConfig where name = 'XmlPackage.RefundEmail')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'XmlPackage.RefundEmail','XMLPACKAGE','Used for the contents of the email sent to customers when their order has been refunded.','notification.orderrefund.xml.config');
    go

    if not exists (select * from dbo.AppConfig where name = 'XmlPackage.OrderCanceledEmail')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'XmlPackage.OrderCanceledEmail','XMLPACKAGE','Used for the contents of the email sent to customers when their order has been voided.','notification.ordercanceled.xml.config');
    go

    if not exists (select * from dbo.AppConfig where name = 'XmlPackage.OrderConfirmationPage')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'XmlPackage.OrderConfirmationPage','XMLPACKAGE','The XmlPackage fired on a successful order received, on the order confirmation page','page.orderconfirmation.xml.config');
    go

    if not exists (select * from dbo.AppConfig where name = 'GatewayRetries')
    INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'GatewayRetries','PAYMENTS','The number of retries to try to call the payment gateway, if it is not responding, times out, or has other connection error. Currently, we only do retries with authorize.net gateway. This value probably should never exceed 3. It should also rarely be needed. Most modern gateways are redundant and very reliable.','2');
    go

    if not exists (select * from dbo.AppConfig where name = 'UPS.MinimumPackageWeight')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'UPS.MinimumPackageWeight','RTSHIPPING','This is the minimum package weight in pounds that can be reported to UPS for a shipping quote.  Note: Setting this to a value less than 0.1 could cause UPS to return an error in their response.','0.1');
    go
    if not exists (select * from dbo.AppConfig where name = 'UseStrongPwd')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'UseStrongPwd','GENERAL', 'set to true to enable strong paswords for customers. When true the regular expression stored in the CustomerPwdValidator AppConfig parameter', 'false');
    go
    if not exists (select * from dbo.AppConfig where name = 'CustomerPwdValidator')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'CustomerPwdValidator','GENERAL','A Regular Expression that is used to validate customer passwords.  Test your expression thoroughly before changing this.','^.*(?=.{8,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%\^&\*\(\)_\+\=\-\[\]\{\}\.,<>\?\/\\|~`:;"'']).*$');


    if not exists(select * from dbo.AppConfig where name = 'QUICKCOMMERCE_X_EMAIL') begin
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QUICKCOMMERCE_X_EMAIL','PAYMENTS','Required for AIM Wells Fargo SecureSource Merchants. Leave blank for non Wells Fargo SecureSource users','');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QUICKCOMMERCE_X_Login','PAYMENTS','The login to use if you are using QUICKCOMMERCE. Provided by QUICKCOMMERCE.','');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QUICKCOMMERCE_X_Tran_Key','PAYMENTS','Transaction key provided by QUICKCOMMERCE','');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QUICKCOMMERCE_X_DELIM_DATA','PAYMENTS','Do not change this value. This value is required by AspDotNetStorefront','TRUE');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QUICKCOMMERCE_X_DELIM_CHAR','PAYMENTS','Do not change this value. This value is required by AspDotNetStorefront','|');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QUICKCOMMERCE_X_ENCAP_CHAR','PAYMENTS','Do not change this value. This value is required by AspDotNetStorefront','');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QUICKCOMMERCE_X_RELAY_RESPONSE','PAYMENTS','Do not change this value. This value is required by AspDotNetStorefront','FALSE');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QUICKCOMMERCE_X_EMAIL_CUSTOMER','PAYMENTS','Do not change this value. This value is required by AspDotNetStorefront. We generate our own receipts','FALSE');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QUICKCOMMERCE_X_METHOD','PAYMENTS','Do not change this value. This value is required by AspDotNetStorefront. ','CC');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QUICKCOMMERCE_X_RECURRING_BILLING','PAYMENTS','Do not change this value. This value is required by AspDotNetStorefront. ','FALSE');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QUICKCOMMERCE_X_VERSION','PAYMENTS','Do not change this value. This value is required by AspDotNetStorefront. ','3.1');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QUICKCOMMERCE_LIVE_SERVER','PAYMENTS','Do not change this value','https://secure.authorize.net/gateway/transact.dll');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QUICKCOMMERCE_TEST_SERVER','PAYMENTS','Do not change this value','https://certification.authorize.net/gateway/transact.dll');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'QUICKCOMMERCE_Verify_Addresses','PAYMENTS','Set to true if you want their gateway to validate the customer billing address against the credit card. Note that this may decrease fraud, but may also cause valid transactions to go failed, if they have different punctuation than on their credit card.','TRUE');
    end
    go

    if not exists(select * from dbo.AppConfig where name = 'ValidateCreditCardNumbers')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ValidateCreditCardNumbers','MISC','if true, a javascript validator will fire on any page that requires entry of a credit card number to try to enforce it is valid.','false');
    go
    if not exists(select * from dbo.AppConfig where name = 'Admin_DefaultTaxClassID')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Admin_DefaultTaxClassID','ADMIN','This is the default tax classification for a product, unless you decide to add additional tax classes and override it','1');
    go
    if not exists(select * from dbo.AppConfig where name = 'MaxBadLogins')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'MaxBadLogins','SECURITY','The number of failed logins before the customer account is locked out','3');
    go
    if not exists(select * from dbo.AppConfig where name = 'BadLoginLockTimeOut')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'BadLoginLockTimeOut','SECURITY','Sets the amount of time in minutes that an account will be locked out after the MaxBadLogins threshold has been exceeded.  You can set this to zero to disable bad login lockout altogether','30')
    go

    if not exists(select * from dbo.AppConfig where name = 'AdminPwdChangeDays')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'AdminPwdChangeDays','SECURITY','The maximum days between passwords for admins users','30');
    go
    if not exists(select * from dbo.AppConfig where name = 'Checkout.UseOnePageCheckout')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Checkout.UseOnePageCheckout','GENERAL','Use single page checkout instead of normal multi-step checkout','false')
    go

    if not exists(select * from dbo.AppConfig where name = 'Checkout.UseOnePageCheckout.UseFinalReviewOrderPage')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Checkout.UseOnePageCheckout.UseFinalReviewOrderPage','GENERAL','IF using one page checkout, and IF your order qualifies for one page checkout, this flag indicates whether to take the user to a final "review order" page before sumitting the transaction. if false, they submit the transaction right on the one page checkout page. We do recommend this being true, to allow the customer to review their full final order stats before "charging" them. Some card processors/merchant accounts/ banks may also require this step!','true')
    go


    if not exists(select * from dbo.AppConfig where name = 'FedexShipManager.Enabled')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'FedexShipManager.Enabled','SHIPPING','If you are going to use the FedEx shipping manager to process shipments, turn this appconfig on to true. NOTE: this is not for Real Time Rates. Only for processing shipments via FedEx shipping manager','false');
    go

    if not exists(select * from [dbo].AppConfig WHERE Name='ApplyShippingHandlingExtraFeeToFreeShipping')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ApplyShippingHandlingExtraFeeToFreeShipping','SHIPPING','If true and ShippingHandlingExtraFee is non-zero, then the ShippingHandlingExtraFee will be applied to the shipping cost even when its $0.00.  If false...then it will not be applied with the shipping cost is $0.00','false')
    go

    if exists(select * from dbo.AppConfig where name = 'bool_UseImageResize')
        DELETE [dbo].AppConfig WHERE GroupName='IMAGE_RESIZING'
    go

    if not exists(select * from [dbo].AppConfig WHERE Name='UseImageResize') begin
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'UseImageResize','IMAGERESIZE','Determines whether image resizing will be used.  This can be overridden in any of the size configs through use of the attribute resize (i.e resize:false;).','true')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ProductImg_icon','IMAGERESIZE','Sets the specifications for the product icon image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-configs.','width:150;height:150;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ProductImg_medium','IMAGERESIZE','Sets the specifications for the product medium image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-configs.','width:250;height:250;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ProductImg_large','IMAGERESIZE','Sets the specifications for the product large image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), croph(left,right, or center), largecreates(true or false), and largeoverwrites(true or false).  All attributes are optional and if not specified will use the values from the default-configs.','width:500;height:500;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ProductImg_swatch','IMAGERESIZE','Sets the specifications for the product swatch image if uploaded manually.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-configs.','width:150;height:50;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'VariantImg_icon','IMAGERESIZE','Sets the specifications for the variant icon image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-configs.','width:150;height:150;')	
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'VariantImg_medium','IMAGERESIZE','Sets the specifications for the variant medium image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-configs.','width:250;height:250;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'VariantImg_large','IMAGERESIZE','Sets the specifications for the variant large image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center), largecreates(true or false), and largeoverwrites(true or false).  All attributes are optional and if not specified will use the values from the default-configs.','width:500;height:500;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'CategoryImg_icon','IMAGERESIZE','Sets the specifications for the category icon image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-configs.','width:150;height:150;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'CategoryImg_medium','IMAGERESIZE','Sets the specifications for the category medium image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-configs.','width:250;height:250;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'CategoryImg_large','IMAGERESIZE','Sets the specifications for the category large image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), croph(left,right, or center), largecreates(true or false), and largeoverwrites(true or false).  All attributes are optional and if not specified will use the values from the default-configs.','width:500;height:500;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ManufacturerImg_icon','IMAGERESIZE','Sets the specifications for the manufacturer icon image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-configs.','width:150;height:150;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ManufacturerImg_medium','IMAGERESIZE','Sets the specifications for the manufacturer medium image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-configs.','width:250;height:250;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ManufacturerImg_large','IMAGERESIZE','Sets the specifications for the manufacturer large image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), croph(left,right, or center), largecreates(true or false), and largeoverwrites(true or false).  All attributes are optional and if not specified will use the values from the default-configs.','width:500;height:500;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'SectionImg_icon','IMAGERESIZE','Sets the specifications for the department icon image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-configs.','width:150;height:150;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'SectionImg_medium','IMAGERESIZE','Sets the specifications for the department medium image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-configs.','width:250;height:250;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'SectionImg_large','IMAGERESIZE','Sets the specifications for the department large image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), croph(left,right, or center), largecreates(true or false), and largeoverwrites(true or false).  All attributes are optional and if not specified will use the values from the default-configs.','width:500;height:500;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DistributorImg_icon','IMAGERESIZE','Sets the specifications for the distributor icon image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-configs.','width:150;height:150;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DistributorImg_medium','IMAGERESIZE','Sets the specifications for the distributor medium image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-configs.','width:250;height:250;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DistributorImg_large','IMAGERESIZE','Sets the specifications for the distributor large image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), croph(left,right, or center), largecreates(true or false), and largeoverwrites(true or false).  All attributes are optional and if not specified will use the values from the default-configs.','width:500;height:500;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'LargeCreatesOthers','IMAGERESIZE','Determines whether an uploaded large image will create the icon and medium images.  This value can be over written in each of the size-configs through use of the attribute largecreates (i.e. largecreates:false;).','true')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'LargeOverwritesOthers','IMAGERESIZE','Determines whether an uploaded large image will create AND overwrite existing icon and medium images.  This value can be over written in each of the size-configs through use of the attribute largeoverwrites (i.e. largeoverwrites:false;).','true')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DefaultCrop','IMAGERESIZE','true or false.  You can leave the crop attribute out of all size-configs and cropping will be determined according to this value.  If you use the crop attribute in the other appconfigs (i.e crop:false;) it will take precedence over this value.  This value should NOT be left blank.','true')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DefaultCropVertical','IMAGERESIZE','top, bottom, or middle.  The vertical anchor point when cropping will default to this unless otherwise specified in the size-configs (i.e cropv:top).','middle')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DefaultCropHorizontal','IMAGERESIZE','left, right, or center.  The horizontal anchor point when cropping will default to this unless otherwise specified in the size-configs (i.e croph:left).','center')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DefaultFillColor','IMAGERESIZE','Default fill color to be used if fill attribute is left out of the other size-configs (i.e fill:#00FF00;).','#FFFFFF')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DefaultQuality','IMAGERESIZE','Default quality if quality attribute is not used in other size-configs (i.e. quality:75;).','100')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DefaultStretch','IMAGERESIZE','Default stretch value if stretch attribute is not specified in other size-configs (i.e. stretch:false).  Stretch is the value that determines whether a smaller, uploaded image will stretch to fill a largeer, resized destination image.','true')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DefaultWidth_icon','IMAGERESIZE','Default width of an icon image if no width attribute is specified in the other size-configs (i.e. width:50;).  This value should NOT be left blank.','150')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DefaultHeight_icon','IMAGERESIZE','Default height of an icon image if no width attribute is specified in the other size-configs (i.e. width:50;).  This value should NOT be left blank.','150')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DefaultWidth_medium','IMAGERESIZE','Default width of a medium image if no width attribute is specified in the other size-configs (i.e. width:50;).  This value should NOT be left blank.','250')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DefaultHeight_medium','IMAGERESIZE','Default height of a medium image if no width attribute is specified in the other size-configs (i.e. width:50;).  This value should NOT be left blank.','250')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DefaultWidth_large','IMAGERESIZE','Default width of a large image if no width attribute is specified in the other size-configs (i.e. width:50;).  This value should NOT be left blank.','500')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DefaultHeight_large','IMAGERESIZE','Default height of a large image if no width attribute is specified in the other size-configs (i.e. width:50;).  This value should NOT be left blank.','500')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'MultiColorMakesSwatchAndMap','IMAGERESIZE','If true, when uploading images in the medium multi-image manager the store will create a single swatch image that is composed of all of your images.  The width and height specified in SwatchStyleAuto are the width and height for each smaller portion of the swatch image and they will be joined into 1 image that is width x number of colors wide and height high.  This will also create the swatch map code for you and insert it into the database.','false')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'MultiMakesMicros','IMAGERESIZE','If true this will create micro images resized by the width and height specified in MicroStyle and will save them in the images/product/micro folder whenever you are uploading multiple images in the medium multi-image manager.  If a product has multi-images and UseImagesForMultiNav is true then images will be shown instead of the number icons.','true')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'MicroStyle','IMAGERESIZE','Attributes used for MultiMakesMicros.  The cols colspacing, and rowspacing attributes are used to determine how many images can appear in each row and how much space (in pixels) is between each image while the width and height determine the resized micro height.','width:40;height:40;cols:5;colspacing:5;rowspacing:5;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'SwatchStyleAuto','IMAGERESIZE','Attributes used for MultiColorMakesSwatchAndMap.  Valid attributes are width, and height.','width:25;height:25;')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'UseImagesForMultiNav','IMAGERESIZE','If true micro images will be used instead of the number icons when multiple images exist in the multi-image manager.','false')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'UseRolloverForMultiNav','IMAGERESIZE','If true and UseImagesForMultiNav is true then the medium image will change when you rollover the micro images instead of when you click on them.','false')
    end
    go


    if not exists(select * from dbo.AppConfig where name = 'BulkImportSendsShipmentNotifications')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'BulkImportSendsShipmentNotifications','SHIPPING','If true, then when bulk importing shipments that arent voided, shipped notification emails will be sent to the customers of the orders whose shipments were imported.','true');
    go




    if not exists(select * from dbo.AppConfig where name = 'TurnOffHtmlEditorInAdminSite')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'TurnOffHtmlEditorInAdminSite','GENERAL','if you do not want to use the Innova HTML editor in your admin site (if it is not understanding your html properly), you can disable it, and show all html enabled fields as simple text areas if you prefer. If fals, the HTML editor will be shown. If true, textarea plain will be shown (you can still enter HTML into the text area)','false')
    go

    if not exists(select * from dbo.AppConfig where name = 'Admin.EntityFrameMenuWidth')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Admin.EntityFrameMenuWidth','GENERAL','The width, in pixels, of the admin site left entity tree pane','350')
    go


    if not exists(select * from dbo.AppConfig where name = 'Admin.ShowSecurityFeed')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Admin.ShowSecurityFeed','GENERAL','If you want to show the security panel on the admin home page, leave this value set to true','true')
    go

    if not exists(select * from dbo.AppConfig where name = 'Admin.ShowNewsFeed')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Admin.ShowNewsFeed','GENERAL','If you want to show the news panel on the admin home page, leave this value set to true','true')
    go

    if not exists(select * from dbo.AppConfig where name = 'Admin.ShowSponsorFeed')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Admin.ShowSponsorFeed','GENERAL','If you want to show the sponsor panel on the admin home page, leave this value set to true','true')
    go


    if not exists(select * from dbo.AppConfig where name = 'AddressCCSaltField')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'AddressCCSaltField','HIDDEN','The salt field to use for encrypting the credit card field in the address table, if AppConfig:StoreCCInDB=true also. The allowed values ONLY are one of the following: AddressID, CustomerID','AddressID')
    go

    if not exists(select * from dbo.AppConfig where name = 'OrdersCCSaltField')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'OrdersCCSaltField','HIDDEN','The salt field to use for encrypting the credit card field in the orders table, if AppConfig:StoreCCInDB=true also. The allowed values ONLY are one of the following: OrderNumber,OrderGUID,CustomerID,CustomerGUID,EMail','EMail')
    go

    if not exists(select * from dbo.AppConfig where name = 'Web.Config.EncryptionProvider')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Web.Config.EncryptionProvider','SECURITY','The encryption provider used to encrypt the web.config.  The allowed values are DataProtectionConfigurationProvider and RsaProtectedConfigurationProvider.  Use RsaProtectedConfigurationProvider is you are running on a web farm.  You will need to create and RSA key container before implementing the RSA encryption provider.','DataProtectionConfigurationProvider');
    go

    if not exists(select * from dbo.AppConfig where name = 'SessionTimeoutInMinutes')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'SessionTimeoutInMinutes','SECURITY','Customer session data timeout value. Default is 15 minutes','15');
    go

    if exists(select * from dbo.AppConfig where name = 'TaxOnShipping')
        delete from [dbo].AppConfig where Name='TaxOnShipping';
    go

    if not exists(select * from dbo.AppConfig where name = 'HSBC.DocVersion') begin
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'HSBC.DocVersion','GATEWAY','Do not change this value','1.0');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'HSBC.ClientID','GATEWAY','Your HSBC Assigned client id. Use only ONE of either the ClientAlias or ClientID. Do NOT fill in both fields. If both are filled in, CLientID is used.','');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'HSBC.ClientAlias','GATEWAY','Your HSBC Assigned client alias. Use only ONE of either the ClientAlias or ClientID. Do NOT fill in both fields. If both are filled in, CLientID is used.','');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'HSBC.ClientName','GATEWAY','Your HSBC Assigned client name (HSBC may call this your User ID)','');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'HSBC.ClientPassword','GATEWAY','Your HSBC Assigned password','');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'HSBC.Pipeline','GATEWAY','Allowed values are Payment or PaymentNoFraud','PaymentNoFraud');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'HSBC.Mode.Test','GATEWAY','Do not change this value unless you know what you are doing, and are working with the HSBC API specifications','R');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'HSBC.Mode.Live','GATEWAY','Do not change this value unless you know what you are doing, and are working with the HSBC API specifications','P');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'HSBC.Test.Server','GATEWAY','The HSBC assigned url to post the transaction to for test mode transactions','https://www.uat.apixml.netq.hsbc.com');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'HSBC.Live.Server','GATEWAY','The HSBC assigned url to post the transaction to for live mode transactions','https://www.uat.apixml.netq.hsbc.com');
    end
    go

    if not exists(select * from dbo.AppConfig where name = 'IPAddress.MaxFailedTransactions')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'IPAddress.MaxFailedTransactions','SECURITY','If an IP Address has had more than this number of failed transactions, it will be added to the restricted IP Address list (table) and orders from that IP address will not be allowed. They will not even be sent to the gateway. An "Order Declined, Please Contact Us" message will just be sent back to the user on the site.','5');
    go

    if not exists(select * from dbo.AppConfig where name = 'IPAddress.RefuseRestrictedIPsFromSite')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'IPAddress.RefuseRestrictedIPsFromSite','SECURITY','if true, then any user that comes from an IP address that is in the RestrictedIP table will be refused to view any page of the site. we fail the page hard with an exception "REFUSED"','true');
    go

    if not exists(select * from dbo.AppConfig where name = 'AdminAlert.Message') begin
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'AdminAlert.Message','GENERAL','If you have to present a message to all site visitors, enter it here. You must also then have an invocation of the skin.adminalert.xml.config XmlPackage in your skin file somewhere, or the message will not be displayed. The message is only displayed if not emtpy (blank)','');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'AdminAlert.BackgroundColor','GENERAL','The background color for the admin alert message','FFFF00');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'AdminAlert.FontColor','GENERAL','The text foreground color for the admin alert message','FF0000');
    end
    go

    update customer set Over13Checked=1 where IsAdmin<>0;
    go

    -- v7.0 added:
    if not exists(select * from dbo.AppConfig where name = 'VAT.DefaultSetting') begin
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'VAT.DefaultSetting','VAT','This sets the default behavior of the storefront for EU customers needing to show prices either VAT inclusive or VAT exclusive. Allowed values are 1 (VAT inclusive) or 2 (VAT exclusive). Enter only a 1 or 2 in the value for this AppConfig','1');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'VAT.AllowCustomerToChooseSetting','VAT','For EU VAT customers, this setting controls whether or not they can on the site choose to view the site in VAT inclusive or VAT exclusive mode for all prices. If false, they will not be able to choose their preference, and the store will force your AppConfig:VAT.DefaultSetting value to be used.','true');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'VAT.Enabled','VAT','Set this to true if you want the store to know about and use EU VAT Tax display costs','false');
    end
    go

    if not exists (select * from dbo.AppConfig where name = 'InitializationVector') begin
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'InitializationVector','ENCRYPTION','You can change it if you want, it has to be exactly 16 characters long! It is not needed to protect it''s value that you choose like you will the EncryptKey.','834!#$5dle40m34A');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'EncryptIterations','ENCRYPTION','Enter a number from 1 to 4. 1 is less secure, but faster, 4 is more secure, but slower','2');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'KeySize','ENCRYPTION','Has to be 128, 192, or 256. Lower values are faster, and less secure. Higher values are more secure','256');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'HashAlgorithm','ENCRYPTION','Must be either MD5 or SHA1 (SHA1 is recommended)','SHA1');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'NextKeyChange','ENCRYPTION','DO NOT CHANGE THIS VALUE',convert(varchar(12), dateadd(dy, 90, getdate()), 1));
    end 
    go

    if not exists (select * from dbo.AppConfig where name = 'GoogleCheckout.AuthenticateCallback') begin
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.AuthenticateCallback','GOOGLE CHECKOUT','Set this to true to verify http basic authentication on the gcCallback.aspx file.','false');
    end
    go



    if not exists(select * from dbo.AppConfig where name = 'ShippingTaxClassID')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ShippingTaxClassID','TAXES','This must match the tax class id of your "shipping"" class, this is sometimes necessary to set tax rates for "shipping" by country, state, or zip','3');
    go

    if not exists(select * from dbo.AppConfig where name = 'VAT.CountryID')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'VAT.CountryID','VAT','This must be set to the CountryID (you can view the country id''s in your admin site under Taxes -> Edit Countries) for your storefront. This is used to set the default country for VAT calculations','0');
    go

    if not exists (select * from dbo.AppConfig where name = 'CYBERSOURCE.merchantID') begin
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'CYBERSOURCE.merchantID','GATEWAY','your merchant id, which is really your vendor id, call cybersource if unsure','');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'CYBERSOURCE.keysDirectory','GATEWAY','should be full physical file path with ending backslash','');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'CYBERSOURCE.keyFilename','GATEWAY','full file name 1111111.p12 for example','');
    end
    go

    if not exists(select * from dbo.AppConfig where name = 'CYBERSOURCE.LiveURL')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'CYBERSOURCE.LiveURL','GATEWAY','Do not change this unless you know what you are doing.','https://ics2ws.ic3.com/commerce/1.x/transactionProcessor');
    go

    if not exists(select * from dbo.AppConfig where name = 'CYBERSOURCE.PITURL')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'CYBERSOURCE.PITURL','GATEWAY','Do not change this unless you know what you are doing.','https://pit.ic3.com/commerce/1.x/transactionProcessor');
    go

    if not exists(select * from dbo.AppConfig where name = 'CYBERSOURCE.paCountryCode')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'CYBERSOURCE.paCountryCode','GATEWAY','Usually this is blank because Cybersource will have it configured on your account. If Cybersource requests you to send a Country Code for Payer Authorization processing, enter it here.','');
    go

    if not exists(select * from dbo.AppConfig where name = 'CYBERSOURCE.paMerchantName')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'CYBERSOURCE.paMerchantName','GATEWAY','Usually this is blank because Cybersource will have it configured on your account. If Cybersource requests you to send a Merchant Name for Payer Authorization processing, enter it here.','');
    go

    if not exists(select * from dbo.AppConfig where name = 'CYBERSOURCE.paMerchantURL')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'CYBERSOURCE.paMerchantURL','GATEWAY','Usually this is blank because Cybersource will have it configured on your account. If Cybersource requests you to send a Merchant URL for Payer Authorization processing, enter it here.','');
    go

    if not exists(select * from dbo.AppConfig where name = 'CYBERSOURCE.UsePIT')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'CYBERSOURCE.UsePIT','GATEWAY','For VBV PIT Testing. This will force the PITURL to be used for requests instead of the TestURL or LiveURL. This should always be false in production environments.','false');
    go

    if not exists (select * from dbo.AppConfig where name = 'CYBERSOURCE.TestURL') 
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'CYBERSOURCE.TestURL','GATEWAY','Cybersource gateway test server','https://ics2wstest.ic3.com/commerce/1.x/transactionProcessor');
    go



    if not exists(select * from dbo.AppConfig where name = 'VIAKLIX_LIVE_SERVER')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'VIAKLIX_LIVE_SERVER','GATEWAY','Do not change unless instructed to by viaKLIX','https://www.viaKLIX.com/process.asp');
    go
    if not exists(select * from dbo.AppConfig where name = 'VIAKLIX_SSL_MERCHANT_ID')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'VIAKLIX_SSL_MERCHANT_ID','GATEWAY','Your viaKLIX assigned Merchant ID','');
    go
    if not exists(select * from dbo.AppConfig where name = 'VIAKLIX_SSL_PIN')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'VIAKLIX_SSL_PIN','GATEWAY','The PIN that corresponds with the specified User ID','');
    go
    if not exists(select * from dbo.AppConfig where name = 'VIAKLIX_SSL_USER_ID')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'VIAKLIX_SSL_USER_ID','GATEWAY','Your viaKLIX User ID for processing Internet transactions','');
    go
    if not exists(select * from dbo.AppConfig where name = 'VIAKLIX_TEST_SERVER')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'VIAKLIX_TEST_SERVER','GATEWAY','Do not change unless instructed to by viaKLIX','https://demo.viaklix.com/process.asp');
    go

    if not exists(select * from dbo.AppConfig where name = 'IDEPOSIT_TEST_URL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'IDEPOSIT_TEST_URL','GATEWAY','Do not change unless instructed to by iDeposit','https://ideposit.vbprograms.net/servlet/pg');
    go
    if not exists(select * from dbo.AppConfig where name = 'IDEPOSIT_TEST_USERNAME')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'IDEPOSIT_TEST_USERNAME','GATEWAY','Your test iDeposit Username','');
    go
    if not exists(select * from dbo.AppConfig where name = 'IDEPOSIT_TEST_PASSWORD')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'IDEPOSIT_TEST_PASSWORD','GATEWAY','Your test iDeposit Password','');
    go
    if not exists(select * from dbo.AppConfig where name = 'IDEPOSIT_TEST_CLERKID')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'IDEPOSIT_TEST_CLERKID','GATEWAY','Your test iDeposit Clerk ID to be associated with transactions (optional)','Internet');
    go
    if not exists(select * from dbo.AppConfig where name = 'IDEPOSIT_LIVE_URL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'IDEPOSIT_LIVE_URL','GATEWAY','Do not change unless instructed to by iDeposit','https://ideposit.vbprograms.net/servlet/pg');
    go
    if not exists(select * from dbo.AppConfig where name = 'IDEPOSIT_LIVE_USERNAME')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'IDEPOSIT_LIVE_USERNAME','GATEWAY','Your iDeposit Username','');
    go
    if not exists(select * from dbo.AppConfig where name = 'IDEPOSIT_LIVE_PASSWORD')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'IDEPOSIT_LIVE_PASSWORD','GATEWAY','Your iDeposit Password','');
    go
    if not exists(select * from dbo.AppConfig where name = 'IDEPOSIT_LIVE_CLERKID')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'IDEPOSIT_LIVE_CLERKID','GATEWAY','Your iDeposit Clerk ID to be associated with transactions (optional)','Internet');
    go

    if not exists(select * from dbo.AppConfig where name = '3DSecure.CreditCardTypeIDs')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'3DSecure.CreditCardTypeIDs','GATEWAY','Comma separated list of CreditCardTypeIDs that 3-D Secure transactions can be processed for with the currently active gateway. You need to get the desired card type ids from the Admin -> Misc -> More -> Credit Card Types.','');
    go

    if not exists(select * from dbo.AppConfig where name = 'Search_ShowGenresInResults')
    begin
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Search_ShowGenresInResults','SEARCH','if true, then a Genre matches will be shown in the advanced search results','false');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'SearchAdv_ShowGenre','SEARCH','if true, then a Genre filter will be shown in the advanced search page','true');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Default_GenrePageSize','SITEDISPLAY','The default pagesize for a Genre, when it is first added to the db. After adding, you can edit in the admin site. Set to 0 for no paging (i.e. all products are listed). This value MAY be used by the XmlPackage displaying this page.','20');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Default_GenreColWidth','SITEDISPLAY','The default ColWidth for a Genre, when it is first added to the db. After adding, you can edit in the admin site. Set to 0 for no paging (i.e. all products are listed). This value MAY be used by the XmlPackage displaying this page.','4');
    end
    go



    if not exists(select * from dbo.AppConfig where name = 'Tree.ShowGenres')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Tree.ShowGenres','SITEDISPLAY','if you want genres to be shown in the tree, set this to true.','true');
    go

    if not exists(select * from dbo.AppConfig where name = 'Tree.ShowVectors')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Tree.ShowVectors','SITEDISPLAY','if you want vectors to be shown in the tree, set this to true.','true');
    go

    if not exists(select * from dbo.AppConfig where name = 'QBMERCHANTSERVICES_ApplicationLogin')
    begin
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'QBMERCHANTSERVICES_ApplicationLogin','GATEWAY','Your Application login that you obtained from QuickBooks. See manual for instructions.','');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'QBMERCHANTSERVICES_ApplicationID','GATEWAY','Your Application ID that you obtained from QuickBooks. See manual for instructions.','');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'QBMERCHANTSERVICES_ApplicationVersion','GATEWAY','do not change this value','1.0');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'QBMERCHANTSERVICES_InstallID','GATEWAY','','');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'QBMERCHANTSERVICES_ConnectionTicket','GATEWAY','Your connection ticket assigned by QuickBooks. See Quick Books Merchant Services and our manual page for instructions on how to obtain your connection ticket.','');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'QBMERCHANTSERVICES_Language','GATEWAY','Your connection ticket assigned by QuickBooks. See Quick Books Merchant Services for instructions on how to obtain your connection ticket.','English');
    end
    go

    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.store_id')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.store_id','GATEWAY','ESelectPlus Store ID','monusqa002')
    go
    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.api_token')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.api_token','GATEWAY','ESelectPlus API Token','qatoken')
    go
    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.host')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.host','GATEWAY','ESelectPlus Host','esplusqa.moneris.com')
    go
    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.commcard_invoice')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.commcard_invoice','GATEWAY','ESelectPlus CommCard Invoice','')
    go
    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.commcard_tax_OrderTotal')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.commcard_tax_OrderTotal','GATEWAY','ESelectPlus Tax Order Total','')
    go
     
    if not exists(select * from dbo.AppConfig where name = 'AnonCheckoutReqEmail')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'AnonCheckoutReqEmail','GENERAL','Boolean value (true or false) that indicates whether anonymous checkout requires email or not.','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.DefaultTaxRate')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.DefaultTaxRate','GOOGLE CHECKOUT','Set this to the deafult tax rate that will be used either for all products or for products that don''t have a tax table mapping','0');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.UseTaxTables')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.UseTaxTables','GOOGLE CHECKOUT','Boolean value (true or false) that indicates whether to use ONLY the deafult tax rate or to implement full table table mappng for google checkout (for non US GoogleCheckout only)','false');
    go


    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.ShippingIsTaxed')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.ShippingIsTaxed','GOOGLE CHECKOUT','Boolean value (true or false) that indicates whether shipping is taxed','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'NewPwdAllowedChars')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'NewPwdAllowedChars','SECURITY','Used when created new passwords via the request new password function on the signin page','abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ23456789~!@#$%&*');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.SandBoxCheckoutButton')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'GoogleCheckout.SandBoxCheckoutButton','GOOGLE CHECKOUT','The url for the GoogleCheckout Sandbox checkout button, the replace token in the string is for the merchantid','http://sandbox.google.com/checkout/buttons/checkout.gif?merchant_id={0}&w=180&h=46&style=white&variant=text');
    go
    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.LiveCheckoutButton')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'GoogleCheckout.LiveCheckoutButton','GOOGLE CHECKOUT','The url for the GoogleCheckout Live checkout button, the replace token in the string is for the merchantid','http://checkout.google.com/buttons/checkout.gif?merchant_id={0}&w=180&h=46&style=white&variant=text');
    go
    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.SandBoxCheckoutURL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'GoogleCheckout.SandBoxCheckoutURL','GOOGLE CHECKOUT','The URL that your website uses to communicate with Google Checkout Sandbox interface, the replace token in the string is for the merchantid','https://sandbox.google.com/checkout/cws/v2/Merchant/{0}/request');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.AllowAnonCheckout')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.AllowAnonCheckout','GOOGLE CHECKOUT','This enables customers to checout with Google Checkout without registering being a register customer (i.e. anonymous customer).  When you set this to true you also need to set the AllowCustomerDuplicateEMailAddresses AppConfig parmeter to true.','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'IATS.URL') begin
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'IATS.URL','GATEWAYS','The web site url for the IATS ticketmaster payment gateway','www.iats.ticketmaster.com');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'IATS.AgentCode','GATEWAYS','Your IATS ticketmaster payment gateway assigned Agent Code','');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'IATS.Password','GATEWAYS','Your IATS ticketmaster payment gateway assigned password','');
    end
    go
     
    if not exists(select * from dbo.AppConfig where name = 'TemplateSwitching.Enabled')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'TemplateSwitching.Enabled','DISPLAY','If you want to enable template file switching by entity, set this flag to true. You can then assign a template.ascx file to each entity (category, section, etc) that you want. The template.ascx file you assign to each entity must exist in the active skin folder (e.g. skins/skin_1).','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'MaxMind.SOAPURL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'MaxMind.SOAPURL','GATEWAY','SOAP MaxMind Web Service URL. Consult MaxMind.com for documentation','http://www.maxmind.com:8005/maxmind/minfraud_soap');
    go

    if not exists(select * from dbo.AppConfig where name = 'TurnOffStoreAdminEMailNotifications')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'TurnOffStoreAdminEMailNotifications','MISC','If false, the store admin will receive a "new order notification" e-mail from the storefront. If true, no e-mail will be sent to the store admin. Customers will also continue to receive their e-mail receipts no matter what this AppConfig value is set to!','true');
    go

    if not exists(select * from dbo.AppConfig where name = 'PinnaclePayments.UserName')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PinnaclePayments.UserName','GATEWAY','Your Pinnacle Payments SmartPayment assigned account username','TestTerminal');
        go

    if not exists(select * from dbo.AppConfig where name = 'PinnaclePayments.Password')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PinnaclePayments.Password','GATEWAY','Your Pinnacle Payments SmartPayment assigned account password','TestPass');
    go

    if not exists(select * from dbo.AppConfig where name = 'PinnaclePayments.SOAPURL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PinnaclePayments.SOAPURL','GATEWAY','Your Pinnacle Payments SmartPayment assigned gateway URL','https://www.ppscommerce.net/SmartPayments/transact.asmx');
    go

    if not exists(select * from dbo.AppConfig where name = 'AllowAddressChangeOnCheckoutShipping')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'AllowAddressChangeOnCheckoutShipping','MISC','If true, the customer will be allowed to select or add a new address easily on the checkout shipping page. If false, they must go through their address book to make address changes.','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'VAT.RoundPerItem')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'VAT.RoundPerItem','VAT','Turns on rounding of the VAT included price before multiplying by the quantity ordered','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'VAT.HideTaxInOrderSummary')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'VAT.HideTaxInOrderSummary','VAT','When set to true, the Tax in the order summary on checkout pages will be hidden when customers have VAT inclusive prices enabled ','true');
    go


    if not exists(select * from dbo.AppConfig where name = 'VerifyAddressesProvider')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'VerifyAddressesProvider','ADMIN','Leave blank if you do not want to verify addresses. Otherwise set it to "usps" and make sure the VerifyAddressesProvider.USPS server and userid are set.','');
    go
    if not exists(select * from dbo.AppConfig where name = 'VerifyAddressesProvider.USPS.Server')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'VerifyAddressesProvider.USPS.Server','ADMIN','USPS server URL for the Verify Address API','http://production.shippingapis.com/shippingapi.dll');
    go
    if not exists(select * from dbo.AppConfig where name = 'VerifyAddressesProvider.USPS.UserID')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'VerifyAddressesProvider.USPS.UserID','ADMIN','USPS userid for the Verify Address API','');
    go

    if not exists(select * from dbo.AppConfig where name = 'CardiaServices.Test.MerchantToken')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'CardiaServices.Test.MerchantToken','GATEWAY','Your Cardia Services assigned test mode merchant token','CF4B6B54-6C28-4FA3-86B0-E9A347D75C6C');
    go
    if not exists(select * from dbo.AppConfig where name = 'CardiaServices.Live.MerchantToken')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'CardiaServices.Live.MerchantToken','GATEWAY','Your Cardia Services assigned live mode merchant token','TBD');
    go
    if not exists(select * from dbo.AppConfig where name = 'CardiaServices.Test.UserToken')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'CardiaServices.Test.UserToken','GATEWAY','Your Cardia Services assigned live mode user token','A3161859-527C-4A85-B1E4-E1AEAD0EE3B7');
    go
    if not exists(select * from dbo.AppConfig where name = 'CardiaServices.Live.UserToken')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'CardiaServices.Live.UserToken','GATEWAY','Your Cardia Services assigned test mode user token','TBD');
    go
    if not exists(select * from dbo.AppConfig where name = 'CardiaServices.SOAPURL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'CardiaServices.SOAPURL','GATEWAY','Your Cardia Services assigned gateway URL','https://secure.cardia.no/Service/Card/Transaction/1.2/Transaction.asmx');
    go
     

    if not exists(select * from dbo.AppConfig where name = 'eWay.Test.CustomerID')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eWay.Test.CustomerID','GATEWAY','Your eWay assigned test mode Customer ID','87654321');
    go
    if not exists(select * from dbo.AppConfig where name = 'eWay.Live.CustomerID')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eWay.Live.CustomerID','GATEWAY','Your eWay assigned live mode Customer ID','TBD');
    go
    if not exists(select * from dbo.AppConfig where name = 'eWay.Test.URL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eWay.Test.URL','GATEWAY','Your eWay assigned gateway test mode posting URL. NOTE: eWay gateway only supports AUTH CAPTURE. Delayed CAPTURE, VOID, and REFUND are not supported via their Xml API.','https://www.eway.com.au/gateway_cvn/xmltest/TestPage.asp');
    go
    if not exists(select * from dbo.AppConfig where name = 'eWay.Live.URL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eWay.Live.URL','GATEWAY','Your eWay assigned gateway live mode posting URL. NOTE: eWay gateway only supports AUTH CAPTURE. Delayed CAPTURE, VOID, and REFUND are not supported via their Xml API.','https://www.eway.com.au/gateway_cvn/xmlpayment.asp');
    go


    if not exists(select * from dbo.AppConfig where name = 'MailMe_UseSSL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'MailMe_UseSSL','EMAIL','Set this to true if your mail server requires an SSL connection.  NOTE:  This is very uncommon and has nothing to do with SSL on your site.  Only change this if you know what you are doing.','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'MailMe_Port')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'MailMe_Port','EMAIL','Set this value to the TCP Port that your mail server uses.  The vast majority of all SMTP servers on the internet use standard port 25.  Only change this if you are certain that your mail server uses a different port for SMTP connections.','25');
    go

    if not exists(select * from dbo.AppConfig where name = 'ShowFullNewsArticle')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ShowFullNewsArticle','GENERAL','If true the entire news article text will be displayed on the news.aspx page, if false then only a link to the article is displayed and the user has to click the link to view the entire article','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'NewsTeaser')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'NewsTeaser','GENERAL','Displayed at the top of the news page','');
    go


    if not exists(select * from dbo.AppConfig where name = 'PayPal.RequireConfirmedAddress')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.RequireConfirmedAddress','GATEWAY','To require the customer to check out only with a Confirmed PayPal Shipping Address set to true, to allow any address, set to false. It is recommended that this be set to true for Seller Protection.','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.DefaultLocaleCode')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.DefaultLocaleCode','GATEWAY','Two character Locale Code for pages displayed by PayPal. Supported values: AU, DE, FR, IT, GB, ES, US','US');
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.Express.AllowAnonCheckout')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Express.AllowAnonCheckout','GATEWAY','This enables customers to checkout using PayPal Express without being a register customer (i.e. anonymous customer). When you set this to true you also need to set the AllowCustomerDuplicateEMailAddresses AppConfig parameter to true.','true');
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.TestServer')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.TestServer','GATEWAY','PayPal Sandbox Site URL. Do not change unless you know what you are doing.','https://www.sandbox.paypal.com/cgi-bin/webscr');
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.PaymentIcon')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.PaymentIcon','GATEWAY','Image URL for Paypal payment method icon.','skins/skin_1/images/paypal2.gif');
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.Promo.Enabled')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Promo.Enabled','GATEWAY','Master control of Promotion. Set to TRUE to enable. You must have approval from PayPal prior to enabling this feature.','False');
    go
    if not exists(select * from dbo.AppConfig where name = 'PayPal.Promo.CartMinimum')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Promo.CartMinimum','GATEWAY','Customer cart must have total greater than or equal to this value to enable promotion','50');
    go
    if not exists(select * from dbo.AppConfig where name = 'PayPal.Promo.CartMaximum')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Promo.CartMaximum','GATEWAY','Customer cart must have total less than or equal to this value to enable promotion','1500');
    go
    if not exists(select * from dbo.AppConfig where name = 'PayPal.Promo.BannerURL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Promo.BannerURL','GATEWAY','URL or path to Banner Image for Promotion','skins/skin_1/images/paypal_promo.gif');
    go
    if not exists(select * from dbo.AppConfig where name = 'PayPal.Promo.LearnMoreURL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Promo.LearnMoreURL','GATEWAY','URL for informational page to display when customer clicks on the Banner.','https://www.paypal.com/cgibin/webscr?cmd=xpt/cps/popup/BCLearnMoreNoPayments-outside');
    go
    if not exists(select * from dbo.AppConfig where name = 'PayPal.Promo.ButtonImageURL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Promo.ButtonImageURL','GATEWAY','URL for Checkout Button Image when cart meets Promotion requirements','https://www.paypal.com/en_US/i/btn/btn_xpressCheckout.gif');
    go
    if not exists(select * from dbo.AppConfig where name = 'PayPal.Promo.Codes')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Promo.Codes','GATEWAY','Promo codes to include for qualifying Express Checkout purchases. Comma Delimited list.','');
    go
    if not exists(select * from dbo.AppConfig where name = 'PayPal.Express.ButtonImageURL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Express.ButtonImageURL','GATEWAY','URL for Express Checkout Button Image','https://www.paypal.com/en_US/i/btn/btn_xpressCheckout.gif');
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.ForceCapture')
        INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'PayPal.ForceCapture','GATEWAY','If true, will force PayPal & PayPal Express Checkout payments to Capture, regardless of TransactionMode. If false, these payments will obey the TransactionMode setting.','false');
    go




    if not exists(select * from dbo.AppConfig where name = 'PayflowPro.Reporting.LiveURL')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayflowPro.Reporting.LiveURL','GATEWAY','Payflow URL for reporting API interface for Live transactions.','https://payments-reports.paypal.com/reportingengine');
    go
    if not exists(select * from dbo.AppConfig where name = 'PayflowPro.RecurringMaxFailPayments')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayflowPro.RecurringMaxFailPayments','GATEWAY','Payflow recurring billing API maxfailpayments value to use for setting up new subscriptions.','1');
    go
    if not exists(select * from dbo.AppConfig where name = 'PayflowPro.Reporting.ReportName')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayflowPro.Reporting.ReportName','GATEWAY','Payflow report name to use for recurring subscription status.','RecurringBillingReport');
    go
    if not exists(select * from dbo.AppConfig where name = 'PayflowPro.Reporting.TestURL')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayflowPro.Reporting.TestURL','GATEWAY','Payflow URL for reporting API interface for Test transactions.','https://payments-reports.paypal.com/test-reportingengine');
    go


    go
    if not exists(select * from dbo.AppConfig where name = 'RecentAdditionsShowPics')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'RecentAdditionsShowPics','MISC','If true pics will be shown on the recentadditions.aspx page','false');
    go



    if not exists(select * from dbo.AppConfig where name = 'Verisign_CertFileRelativePath')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Verisign_CertFileRelativePath','GATEWAY','If using Verisign PayFlow PRO gateway, this appconfig must be set to the RELATIVE path to your Verisign CERT File (e.g. /certs/f73e89fd.0). Use only a RELATIVE path to the file please','certs');
    go

    if not exists(select * from dbo.AppConfig where name = 'Verisign_DotNet_LiveURL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Verisign_DotNet_LiveURL','GATEWAY','The default Live Verisign Dot Net SDK API post transaction Url. Change only if instructed to by your Verisign account setup instructions','payflowpro.verisign.com/transaction');
    go

    if not exists(select * from dbo.AppConfig where name = 'Verisign_DotNet_TestURL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Verisign_DotNet_TestURL','GATEWAY','The default Test Verisign Dot Net SDK API post transaction Url. Change only if instructed to by your Verisign account setup instructions','pilot-payflowpro.verisign.com/transaction');
    go

    if not exists(select * from dbo.AppConfig where name = 'EntitySelectLists.Enabled')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'EntitySelectLists.Enabled','MISC','If true, Entity Select List (e.g. (!CATEGORY_SELECT_LIST!) tokens will be supported by the parser object','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'Polls.Enabled')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Polls.Enabled','MISC','If true, Poll tokens will be supported by the parser object','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'DefaultCustomerLevelID')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'DefaultCustomerLevelID','MISC','the default customer level (int) to be used on new customer records','0');
    go

    if not exists(select * from dbo.AppConfig where name = 'BreadcrumbSeparator')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'BreadcrumbSeparator','MISC','This is the separator character(s) used when building up breadcrumbs on entity and product pages.','&rarr;');
    go

    if not exists(select * from dbo.AppConfig where name = 'USAePay.Test.SourceKey')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'USAePay.Test.SourceKey','GATEWAY','The test USA ePay source key','dgb8otyulg26vm2hYiF8b2q6P7091681');
    go
    if not exists(select * from dbo.AppConfig where name = 'USAePay.Test.Pin')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'USAePay.Test.Pin','GATEWAY','The test USA ePay pin','ABA123');
    go
    if not exists(select * from dbo.AppConfig where name = 'USAePay.Live.SourceKey')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'USAePay.Live.SourceKey','GATEWAY','Your live USA ePay source key, assigned to you by USA ePay','');
    go
    if not exists(select * from dbo.AppConfig where name = 'USAePay.Live.Pin')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'USAePay.Live.Pin','GATEWAY','Your live USA ePay pin, assigned to you by USA ePay','');
    go
    if not exists(select * from dbo.AppConfig where name = 'USAePay.Description')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'USAePay.Description','GATEWAY','this is the value passed to USAePay to describe every transaction','AspDotNetStorefront Order');
    go
    if not exists(select * from dbo.AppConfig where name = 'USAePay.Declined')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'USAePay.Declined','GATEWAY','Message to be used if the card is declined by the gateway','The Transaction Was Declined');
    go
    if not exists(select * from dbo.AppConfig where name = 'USAePay.TransactionError')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'USAePay.TransactionError','GATEWAY','Message to be used if there is an unknown transaction error from the USA ePay gateway','Unknown Error on USAePay Gateway');
    go
    if not exists(select * from dbo.AppConfig where name = 'USAePay.ConnectionError')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'USAePay.ConnectionError','GATEWAY','Message to be used if we cannot get a connection to the USA ePay gateway','Could not connect to USAePay Gateway');
    go
	if not exists(select * from dbo.AppConfig where name = 'USAePay.UseSandBox')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'USAePay.UseSandBox','GATEWAY','Allows customers to test against their sandbox environment','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'ProtX.UseSimulator')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtX.UseSimulator','GATEWAY','Set to true to use the Simulator URLs. This overrides both the Live and Test URLs.','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'ProtX.Vendor')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtX.Vendor','GATEWAY','Your Vendor name supplied by ProtX.','');
    go

    if not exists(select * from dbo.AppConfig where name = 'ProtXURL.Live.Callback')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Live.Callback','GATEWAY','Do not change this unless you know what you are doing.','https://ukvps.protx.com/VPSDirectAuth/Callback3D.asp');
    go

    if not exists(select * from dbo.AppConfig where name = 'ProtXURL.Live.Purchase')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Live.Purchase','GATEWAY','Do not change this unless you know what you are doing.','https://ukvps.protx.com/VPSDirectAuth/PaymentGateway3D.asp');
    go

    if not exists(select * from dbo.AppConfig where name = 'ProtXURL.Live.Server')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Live.Server','GATEWAY','Do not change this unless you know what you are doing.','https://ukvps.protx.com/vps200/dotransaction.dll');
    go

    if not exists(select * from dbo.AppConfig where name = 'ProtXURL.Simulator.Callback')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Simulator.Callback','GATEWAY','Do not change this unless you know what you are doing.','https://ukvpstest.protx.com/VSPSimulator/VSPDirectCallback.asp');
    go

    if not exists(select * from dbo.AppConfig where name = 'ProtXURL.Simulator.Purchase')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Simulator.Purchase','GATEWAY','Do not change this unless you know what you are doing.','https://ukvpstest.protx.com/VSPSimulator/VSPDirectGateway.asp');
    go

    if not exists(select * from dbo.AppConfig where name = 'ProtXURL.Simulator.Server')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Simulator.Server','GATEWAY','Do not change this unless you know what you are doing.','https://ukvpstest.protx.com/VSPSimulator/VSPServerGateway.asp');
    go

    if not exists(select * from dbo.AppConfig where name = 'ProtXURL.Test.Callback')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Test.Callback','GATEWAY','Do not change this unless you know what you are doing.','https://ukvpstest.protx.com/VPSDirectAuth/Callback3D.asp');
    go

    if not exists(select * from dbo.AppConfig where name = 'ProtXURL.Test.Purchase')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Test.Purchase','GATEWAY','Do not change this unless you know what you are doing.','https://ukvpstest.protx.com/VPSDirectAuth/PaymentGateway3D.asp');
    go

    if not exists(select * from dbo.AppConfig where name = 'ProtXURL.Test.Server')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Test.Server','GATEWAY','Do not change this unless you know what you are doing.','https://ukvpstest.protx.com/vps200/dotransaction.dll');
    go

    if not exists(select * from dbo.AppConfig where name = 'Recurring.UseGatewayInternalBilling')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Recurring.UseGatewayInternalBilling','MISC','if true, the gateway''s internal billing will be used instead of our own build order billing mechanism when processing recurring orders. This is ONLY allowed to be true if you are using the Authorize.net or Verisign PayFlow PRO gateways!! If using those gateways, setting this flag to true allows you to not have to store credit cards in your db. See manual for further instructions on how to process the recurring order reports using each gateway.','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'Recurring.AUTHORIZENET_AUTOBILL_TEST_SERVER')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Recurring.AUTHORIZENET_AUTOBILL_TEST_SERVER','GATEWAY','Do not change this value unless instructed by Authorize.net or our support group','https://apitest.authorize.net/xml/v1/request.api');
    go

    if not exists(select * from dbo.AppConfig where name = 'Recurring.AUTHORIZENET_AUTOBILL_LIVE_SERVER')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Recurring.AUTHORIZENET_AUTOBILL_LIVE_SERVER','GATEWAY','Do not change this value unless instructed by Authorize.net or our support group','https://api.authorize.net/xml/v1/request.api');
    go

    if not exists(select * from dbo.AppConfig where name = 'Recurring.GatewayImportOffsetHours')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Recurring.GatewayImportOffsetHours','MISC','Hours to offset the reporting date from midnight. If you want the report to run through 6:00 AM then set this value to 6.','0');
    go
    if not exists(select * from dbo.AppConfig where name = 'Recurring.GatewayLastImportedDate')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Recurring.GatewayLastImportedDate','MISC','Time stamp for last Gateway Import. This value is automatically updated.','');
    go

    if not exists(select * from dbo.AppConfig where name = 'Recurring.LimitCustomerToOneOrder')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Recurring.LimitCustomerToOneOrder','MISC','When true, if a customer places a new Gateway Auto-Ship order any existing Auto-Ship orders are canceled with the gateway and are cleared from their cart. If a subscription is involved and SubscriptionExtensionOccursFromOrderDate=false then their remaining subscription period will be preserved.','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'Recurring.ClearIsNewFlag')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Recurring.ClearIsNewFlag','MISC','When true, recurrences of Auto-Ship orders will have the IsNew flag cleared automatically.','false');
    go


    if not exists(select * from dbo.AppConfig where name = 'InternationalCheckout.Enabled')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'InternationalCheckout.Enabled','MISC','if true, internationalcheckout.com button will be visible on your shopping cart page. You must sign up separately for an account from International Checkout if you want to use this feature.','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'InternationalCheckout.StoreID')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'InternationalCheckout.StoreID','MISC','if true and InternationalCheckout is enabled, enter your InternationalCheckout.com assigned storeid here (e.g. store123)','');
    go

    if not exists(select * from dbo.AppConfig where name = 'InternationalCheckout.ForceForInternationalCustomers')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'InternationalCheckout.ForceForInternationalCustomers','MISC','if true and InternationalCheckout is enabled, and the customer''s address is outside the U.S., their only checkout button will be the InternationalCheckout button. See AppConfig:InternationalCheckout.Enabled appconfig also.','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'InternationalCheckout.TestMode')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'InternationalCheckout.TestMode','MISC','if true and InternationalCheckout is enabled, you can set this to true to see the form that is submitted to InternationalCheckout before the submission. This can help during debug/development mode','true');
    go

    if not exists(select * from dbo.AppConfig where name = 'ZipCodePrefixLength')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ZipCodePrefixLength','SHIPPING', 'Sets the length used to macth zip code prefixes when matching shipping zones', '3');
    go


    if not exists(select * from dbo.AppConfig where name = 'ShippingTrackingCarriers')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ShippingTrackingCarriers','SHIPPING','Comma delimited list of Carriers that have tracking numbers that can be matched. These values must match up with corresponding ShippingTrackingURL and ShippingTrackingRegEx AppConfig variables.','UPS,USPS,FedEx,DHL,AusPost,ParcelForce');
    go
    if not exists(select * from dbo.AppConfig where name = 'ShippingTrackingRegEx.AusPost')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ShippingTrackingRegEx.AusPost','SHIPPING','Australia Post Tracking Number Match Regular Expression','^[0-9A-Z]{8}$');
    go
    if not exists(select * from dbo.AppConfig where name = 'ShippingTrackingURL.AusPost')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ShippingTrackingURL.AusPost','SHIPPING','Australia Post URL for generating package tracking link. You must include the {0} placeholder at the location in the link that the tracking number should be inserted.','http://platinum.auspost.com.au/scripts/cgiip.exe/WService=wtsaae/ap_inquiryresults.w?inquirynumber={0}');
    go
    if not exists(select * from dbo.AppConfig where name = 'ShippingTrackingRegEx.DHL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ShippingTrackingRegEx.DHL','SHIPPING','DHL Tracking Number Match Regular Expression','^\d{10,11}$');
    go
    if not exists(select * from dbo.AppConfig where name = 'ShippingTrackingURL.DHL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ShippingTrackingURL.DHL','SHIPPING','DHL URL for generating package tracking link. You must include the {0} placeholder at the location in the link that the tracking number should be inserted.','http://track.dhl-usa.com/atrknav.asp?ShipmentNumber={0}');
    go
    if not exists(select * from dbo.AppConfig where name = 'ShippingTrackingRegEx.FedEx')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ShippingTrackingRegEx.FedEx','SHIPPING','FedEx Tracking Number Match Regular Expression','^\d{12,19}$');
    go
    if not exists(select * from dbo.AppConfig where name = 'ShippingTrackingURL.FedEx')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ShippingTrackingURL.FedEx','SHIPPING','FedEx URL for generating package tracking link. You must include the {0} placeholder at the location in the link that the tracking number should be inserted.','http://www.fedex.com/Tracking?tracknumbers={0}');
    go
    if not exists(select * from dbo.AppConfig where name = 'ShippingTrackingRegEx.ParcelForce')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ShippingTrackingRegEx.ParcelForce','SHIPPING','Parcel Force Tracking Number Match Regular Expression','^[0-9A-Z]{11}GB$');
    go
    if not exists(select * from dbo.AppConfig where name = 'ShippingTrackingURL.ParcelForce')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ShippingTrackingURL.ParcelForce','SHIPPING','Parcel Force URL for generating package tracking link. You must include the {0} placeholder at the location in the link that the tracking number should be inserted.','http://www.parcelforce.com/portal/pw/track?catId=7500082&pageId=trt_trackingdetail&itemNumber={0}');
    go
    if not exists(select * from dbo.AppConfig where name = 'ShippingTrackingRegEx.UPS')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ShippingTrackingRegEx.UPS','SHIPPING','UPS Tracking Number Match Regular Expression','^(1Z)');
    go
    if not exists(select * from dbo.AppConfig where name = 'ShippingTrackingURL.UPS')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ShippingTrackingURL.UPS','SHIPPING','UPS URL for generating package tracking link. You must include the {0} placeholder at the location in the link that the tracking number should be inserted.','http://wwwapps.ups.com/WebTracking/processInputRequest?sort_by=status&tracknums_displayed=1&TypeOfInquiryNumber=T&loc=en_US&InquiryNumber1={0}&track.x=0&track.y=0');
    go
    if not exists(select * from dbo.AppConfig where name = 'ShippingTrackingRegEx.USPS')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ShippingTrackingRegEx.USPS','SHIPPING','USPS Tracking Number Match Regular Expression','^\d{22}|[0-9A-Z]{11}US$');
    go
    if not exists(select * from dbo.AppConfig where name = 'ShippingTrackingURL.USPS')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ShippingTrackingURL.USPS','SHIPPING','USPS URL for generating package tracking link. You must include the {0} placeholder at the location in the link that the tracking number should be inserted.','http://trkcnfrm1.smi.usps.com/PTSInternetWeb/InterLabelInquiry.do?origTrackNum={0}');
    go

    if not exists(select * from dbo.AppConfig where name = 'SubscriptionExpiredGracePeriod')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'SubscriptionExpiredGracePeriod','MISC','Number of days before expired subscriptions are enforced and customer loses access.','0');
    go

    if not exists(select * from dbo.AppConfig where name = 'SecureNet.ID')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'SecureNet.ID','GATEWAY','Your SecurenetID supplied by SecureNet.','');
    go
    if not exists(select * from dbo.AppConfig where name = 'SecureNet.Key')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'SecureNet.Key','GATEWAY','Your SecureKey supplied by SecureNet.','');
    go
    if not exists(select * from dbo.AppConfig where name = 'SecureNet.LiveURL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'SecureNet.LiveURL','GATEWAY','SecureNet Gateway Web Service API endpoint for Live transactions.','https://gateway.securenet.com/payment.asmx');
    go
    if not exists(select * from dbo.AppConfig where name = 'SecureNet.TestURL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'SecureNet.TestURL','GATEWAY','SecureNet Gateway Web Service API endpoint for Test transactions.','https://gateway.securenet.com/payment.asmx');
    go


    if not exists(select * from dbo.AppConfig where name = 'ProtX.UseSimulator')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtX.UseSimulator','GATEWAY','Set to true to use the Simulator URLs. This overrides both the Live and Test URLs.','false');
    go
    if not exists(select * from dbo.AppConfig where name = 'ProtX.Vendor')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtX.Vendor','GATEWAY','Your Vendor name supplied by ProtX.','');
    go
    if not exists(select * from dbo.AppConfig where name = 'ProtXURL.Simulator.Release')
    BEGIN
        DELETE from appconfig where name like 'ProtXURL%'
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Live.Abort','GATEWAY','Do not change this unless you know what you are doing.','https://ukvps.protx.com/vspgateway/service/abort.vsp');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Live.Callback','GATEWAY','Do not change this unless you know what you are doing.','https://ukvps.protx.com/vspgateway/service/direct3dcallback.vsp');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Live.Purchase','GATEWAY','Do not change this unless you know what you are doing.','https://ukvps.protx.com/vspgateway/service/vspdirect-register.vsp');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Live.Refund','GATEWAY','Do not change this unless you know what you are doing.','https://ukvps.protx.com/vspgateway/service/refund.vsp');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Live.Release','GATEWAY','Do not change this unless you know what you are doing.','https://ukvps.protx.com/vspgateway/service/release.vsp');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Simulator.Abort','GATEWAY','Do not change this unless you know what you are doing.','https://ukvpstest.protx.com/VSPSimulator/VSPServerGateway.asp?service=VendorAbortTx');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Simulator.Callback','GATEWAY','Do not change this unless you know what you are doing.','https://ukvpstest.protx.com/VSPSimulator/VSPDirectCallback.asp');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Simulator.Purchase','GATEWAY','Do not change this unless you know what you are doing.','https://ukvpstest.protx.com/VSPSimulator/VSPDirectGateway.asp');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Simulator.Refund','GATEWAY','Do not change this unless you know what you are doing.','https://ukvpstest.protx.com/VSPSimulator/VSPServerGateway.asp?service=VendorRefundTx');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Simulator.Release','GATEWAY','Do not change this unless you know what you are doing.','https://ukvpstest.protx.com/VSPSimulator/VSPServerGateway.asp?service=VendorReleaseTx');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Test.Abort','GATEWAY','Do not change this unless you know what you are doing.','https://ukvpstest.protx.com/vspgateway/service/abort.vsp');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Test.Callback','GATEWAY','Do not change this unless you know what you are doing.','https://ukvpstest.protx.com/vspgateway/service/direct3dcallback.vsp');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Test.Purchase','GATEWAY','Do not change this unless you know what you are doing.','https://ukvpstest.protx.com/vspgateway/service/vspdirect-register.vsp');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Test.Refund','GATEWAY','Do not change this unless you know what you are doing.','https://ukvpstest.protx.com/vspgateway/service/refund.vsp');
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ProtXURL.Test.Release','GATEWAY','Do not change this unless you know what you are doing.','https://ukvpstest.protx.com/vspgateway/service/release.vsp');
    END
    go


    if not exists(select * from dbo.AppConfig where name = 'FreeShippingAllowsRateSelection')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values (1,'FreeShippingAllowsRateSelection','SHIPPING','Set to true to allow customers to select a shipping method other than the free method if they want to pay for faster delivery or a different shipping method when the order does not consist of all download items.  Leave false to force the free shipping method...the customer will not be able to choose a different method.','false')
    go


    if not exists(select * from dbo.AppConfig where name = 'OrderEditingEnabled')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values (1,'OrderEditingEnabled','ADMIN','If true, customers and admin users can edit orders, assuming those orders meet state critieria for being editable','false')
    go



    -- Added AppConfigs
    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.DefaultDomesticShipToCity')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.DefaultDomesticShipToCity','GOOGLE CHECKOUT','Used to calculate default shipping rates for merchant calculated shipping in case Google fails to update the shipping rates in real time with the actual customer address.','Wichita');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.DefaultDomesticShipToState')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.DefaultDomesticShipToState','GOOGLE CHECKOUT','Used to calculate default shipping rates for merchant calculated shipping in case Google fails to update the shipping rates in real time with the actual customer address.','KS');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.DefaultDomesticShipToZip')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.DefaultDomesticShipToZip','GOOGLE CHECKOUT','Used to calculate default shipping rates for merchant calculated shipping in case Google fails to update the shipping rates in real time with the actual customer address.','67215');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.DefaultDomesticShipToCountry')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.DefaultDomesticShipToCountry','GOOGLE CHECKOUT','Used to calculate default shipping rates for merchant calculated shipping in case Google fails to update the shipping rates in real time with the actual customer address. Two Letter Code.','US');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.DefaultInternationalShipToCity')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.DefaultInternationalShipToCity','GOOGLE CHECKOUT','Used to calculate default shipping rates for merchant calculated shipping in case Google fails to update the shipping rates in real time with the actual customer address.','Norwich');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.DefaultInternationalShipToState')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.DefaultInternationalShipToState','GOOGLE CHECKOUT','Used to calculate default shipping rates for merchant calculated shipping in case Google fails to update the shipping rates in real time with the actual customer address.','--');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.DefaultInternationalShipToZip')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.DefaultInternationalShipToZip','GOOGLE CHECKOUT','Used to calculate default shipping rates for merchant calculated shipping in case Google fails to update the shipping rates in real time with the actual customer address.','NR1 3QA');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.DefaultInternationalShipToCountry')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.DefaultInternationalShipToCountry','GOOGLE CHECKOUT','Used to calculate default shipping rates for merchant calculated shipping in case Google fails to update the shipping rates in real time with the actual customer address. Two Letter Code.','GB');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.CarrierCalculatedShippingEnabled')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.CarrierCalculatedShippingEnabled','GOOGLE CHECKOUT','When true, Google Checkout calculates the shipping costs. When false, your store front calculates the shipping costs.','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.CarrierCalculatedPackage')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.CarrierCalculatedPackage','GOOGLE CHECKOUT','Carrier Calculated Shipping package dimensions in inches. Example: 10x12x8','10x10x10');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.CarrierCalculatedShippingOptions')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.CarrierCalculatedShippingOptions','GOOGLE CHECKOUT','Comma separated list of Google Checkout Carrier Calculated shipping methods with optional default price. These are case sensitive. The default price can be included with a | (pipe) character and then the amount added after the method name. Valid methods: FedEx 2Day, FedEx Express Saver, FedEx First Overnight, FedEx Ground, FedEx Home Delivery, FedEx Priority Overnight, FedEx Standard Overnight, UPS 2nd Day Air, UPS 2nd Day Air AM, UPS 3 Day Select, UPS Ground, UPS Next Day Air, UPS Next Day Air Early AM, UPS Next Day Air Saver, USPS Express Mail, USPS Media Mail, USPS Parcel Post, USPS Priority Mail','FedEx 2Day|20, FedEx Standard Overnight|30, UPS 2nd Day Air|20, UPS Ground|10, UPS Next Day Air|30, USPS Express Mail|50');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.CarrierCalculatedDefaultPrice')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.CarrierCalculatedDefaultPrice','GOOGLE CHECKOUT','Google could display this to the customer if there is a technical problem. Use a large value. Specific default prices may be set with the GoogleCheckout.CarrierCalculatedShippingOptions appconfig. This value will be used for the Options that do not have it specifed within GoogleCheckout.CarrierCalculatedShippingOptions.','50.0');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.CarrierCalculatedFreeOption')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.CarrierCalculatedFreeOption','GOOGLE CHECKOUT','The shipping method(s) that you want to mark as having free shipping. If more than one, separate with commas. See GoogleCheckout.CarrierCalculatedShippingOptions decription for valid values.','UPS Ground, FedEx Home Delivery');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.ConversionURL')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.ConversionURL','GOOGLE CHECKOUT','Enter the URL used for your third-party conversion tracking. If there are parameters specified in ConversionParameters, those should not appear within this string. It should start with http:// or https://','');
    go

    if not exists(select * from dbo.AppConfig where name = 'GoogleCheckout.ConversionParameters')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'GoogleCheckout.ConversionParameters','GOOGLE CHECKOUT','This setting is optional depending on the requirements of your conversion tracking service. Enter the Google Checkout parameter name followed by a space and then the name to be used in the url for your conversion tracking service. For instance, if your conversion tracking service has a parameter called amount that corresponds to the merchandise amount you would set this value to order-subtotal amount without the quotes. Multiple parameters should be separated by commas. Valid Google Checkout parameter names are: buyer-id, order-id, order-subtotal, order-subtotal-plus-tax, order-subtotal-plus-shipping, order-total, tax-amount, shipping-amount, coupon-amount, billing-city, billing-region, billing-postal-code, billing-country-code, shipping-city, shipping-region, shipping-postal-code, shipping-country-code','');
    go


    if not exists(select * from dbo.AppConfig where name = 'Ogone.PSPID')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Ogone.PSPID','GATEWAY','Ogone PSPID','');
    go
    if not exists(select * from dbo.AppConfig where name = 'Ogone.USERID')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Ogone.USERID','GATEWAY','Ogone USERID for DirectLink access.','');
    go
    if not exists(select * from dbo.AppConfig where name = 'Ogone.PSWD')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Ogone.PSWD','GATEWAY','Ogone password for the USERID user.','');
    go
    if not exists(select * from dbo.AppConfig where name = 'Ogone.SHASignature')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Ogone.SHASignature','GATEWAY','Must match SHA-1 Signature setting in Ogone account configuration.','');
    go
    if not exists(select * from dbo.AppConfig where name = 'Ogone.LivePostURL')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Ogone.LivePostURL','GATEWAY','Live mode URL for 3-Tier e-Commerce new order FORM POST.','https://secure.ogone.com/ncol/prod/orderstandard.asp');
    go
    if not exists(select * from dbo.AppConfig where name = 'Ogone.TestPostURL')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Ogone.TestPostURL','GATEWAY','Test mode URL for 3-Tier e-Commerce new order FORM POST.','https://secure.ogone.com/ncol/test/orderstandard.asp');
    go
    if not exists(select * from dbo.AppConfig where name = 'Ogone.LiveServerOrder')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Ogone.LiveServerOrder','GATEWAY','Live mode URL for DirectLink new order requests.','https://secure.ogone.com/ncol/prod/orderdirect.asp');
    go
    if not exists(select * from dbo.AppConfig where name = 'Ogone.TestServerOrder')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Ogone.TestServerOrder','GATEWAY','Test mode URL for DirectLink new order requests.','https://secure.ogone.com/ncol/test/orderdirect.asp');
    go
    if not exists(select * from dbo.AppConfig where name = 'Ogone.LiveServer')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Ogone.LiveServer','GATEWAY','Live mode URL for DirectLink maintenance requests.','https://secure.ogone.com/ncol/prod/maintenancedirect.asp');
    go
    if not exists(select * from dbo.AppConfig where name = 'Ogone.TestServer')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Ogone.TestServer','GATEWAY','Test mode URL for DirectLink maintenance requests.','https://secure.ogone.com/ncol/test/maintenancedirect.asp');
    go
    if not exists(select * from dbo.AppConfig where name = 'Ogone.Use3TierMode')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Ogone.Use3TierMode','GATEWAY','Default behavior is to use DirectLink for new orders, if you want to use 3-Tier e-Commerce then set this to true. This is not supported when using one page checkout. You must configure Post payment feedback on Ogone.com pointing to ogone_postsale.aspx.','false');
    go




    if not exists(select * from [dbo].AppConfig WHERE Name='RTShipping.MultiDistributorCalculation')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'RTShipping.MultiDistributorCalculation','RTSHIPPING','*BETA* - If you have multiple distributors, set this true to allow real time shipping rates to be calculated for the products based on the address of the distributor that its assigned to.  Currently only available for UPS.  Do not set this to true if using GoogleCheckout - GoogleCheckout is not supported with this set to true.','false')
    go

    if not exists(select * from dbo.AppConfig where name = 'RTShipping.UseTestRates')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'RTShipping.UseTestRates','RTSHIPPING','Used to determine if the realtime rates service contacts live or test servers','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'RTShipping.USPS.Services')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'RTShipping.USPS.Services','RTSHIPPING','Contains a list of available USPS Services. Valid services are: Express,Priority,Parcel,Library,Media','Express,Priority,Parcel,Library,Media');
    go

    -- new AppConfigs for Canada Post and DHL Domestic and International
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.CanadaPost.DefaultPackageSize')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.CanadaPost.DefaultPackageSize','RTSHIPPING','For items combined into one box, this is the default size of that box in cm.','25x20x15');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.CanadaPost.Language')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.CanadaPost.Language','RTSHIPPING','Valid values are en (English), fr (French), auto (default; selected based on customer locale).','auto');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.CanadaPost.MaxWeight')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.CanadaPost.MaxWeight','RTSHIPPING','Canada Post maximum package weight in kg.','30');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.CanadaPost.MerchantID')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.CanadaPost.MerchantID','RTSHIPPING','Sell Online Merchant ID.','');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.CanadaPost.Server')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.CanadaPost.Server','RTSHIPPING','DNS of Sell Online ratings server. Default is sellonline.canadapost.ca.','sellonline.canadapost.ca');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.CanadaPost.ServerPort')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.CanadaPost.ServerPort','RTSHIPPING','TCP port of Sell Online ratings server. Default is 30000.','30000');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.DHL.BillingAccountNbr')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.DHL.BillingAccountNbr','RTSHIPPING','If the BillingParty is someone other than S (Sender), enter the DHL account number to be billed.','');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.DHL.BillingParty')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.DHL.BillingParty','RTSHIPPING','Valid codes are S (Sender, default), R (Receiver), and 3 (Third Party).','S');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.DHL.Overrides')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.DHL.Overrides','RTSHIPPING','Valid codes include Z1 (override Postal Code not in State/Province error), Z2 (override PostalCode not in DHL database error), ES (generate Saturday quote on day other than Friday)','');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.DHL.Packaging')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.DHL.Packaging','RTSHIPPING','Valid codes are P (Package) or L (Letter).','P');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.DHL.Services')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.DHL.Services','RTSHIPPING','Service Level Code for the requested domestic service levels. Valid codes are any of  G;DHL Ground Service,S;DHL 2nd Day Service,N;DHL Next Day 3:00 pm,E;DHL Next Day 12:00 pm,E;DHL Next Day 10:30 am,E;Express Saturday.','G;DHL Ground Service,S;DHL 2nd Day Service,N;DHL Next Day 3:00 pm,E;DHL Next Day 12:00 pm,E;DHL Next Day 10:30 am,E;Express Saturday');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.DHL.ShipInDays')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.DHL.ShipInDays','RTSHIPPING','The number of days from today to calculate the Ship-On date.','2');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.DHLIntl.BillingAccountNbr')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.DHLIntl.BillingAccountNbr','RTSHIPPING','If the IntlBillingParty is someone other than S (Sender), enter the DHL account number to be billed.','');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.DHLIntl.BillingParty')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.DHLIntl.BillingParty','RTSHIPPING','Valid codes are S (Sender, default), R (Receiver), and 3 (Third Party).','S');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.DHLIntl.Dutiable')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.DHLIntl.Dutiable','RTSHIPPING','Indicates if the shipment is dutiable or non-dutiable. Valid codes are Y (dutiable, default) or N (non-dutiable).','Y');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.DHLIntl.DutyPayment')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.DHLIntl.DutyPayment','RTSHIPPING','Duty and tax charge payment type. Valid codes are S (Shipper, default), R (Receiver), or 3 (Third Party).','S');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.DHLIntl.DutyPaymentAccountNbr')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.DHLIntl.DutyPaymentAccountNbr','RTSHIPPING','If Duty payment type is someone other than Sender or Receiver, enter the DHL account number to be billed.','');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.DHLIntl.MaxWeight')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.DHLIntl.MaxWeight','RTSHIPPING','DHL International maximum package weight.','550');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.DHLIntl.Overrides')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.DHLIntl.Overrides','RTSHIPPING','Valid codes include Z1 (override Postal Code not in State/Province error), Z2 (override PostalCode not in DHL database error), ES (generate Saturday quote on day other than Friday), RP (destination PostalCode optional)','');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.DHLIntl.Packaging')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.DHLIntl.Packaging','RTSHIPPING','Valid codes are O (Your Packaging, default), L (DHL Express Envelope,max 8 oz), P (DHL Other Packaging)','O');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.DHLIntl.Services')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.DHLIntl.Services','RTSHIPPING','Service Level Code for the requested service levels. Valid codes are IE;DHL Worldwide Priority Express','IE;DHL Worldwide Priority Express');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.DHLIntl.ShippingKey')
     INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.DHLIntl.ShippingKey','RTSHIPPING','Your DHL Shipping Key for International Shipments','');
    go

    if not exists(select * from dbo.AppConfig where name = 'RTShipping.AusPost.DefaultPackageSize')
        INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.AusPost.DefaultPackageSize','RTSHIPPING','For non-ShipSeparately items, the package dimensions to use, specified in cm. ','15x15x15');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.AusPost.DomesticServices')
        INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.AusPost.DomesticServices','RTSHIPPING','List of domestic shipping classes, separated by commas. Each entry consists of a CODE;Description pair, separated by a semi-colon.','STANDARD;Regular Parcel, EXPRESS;Express Post Parcel');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.AusPost.IntlServices')
        INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.AusPost.IntlServices','RTSHIPPING','List of international shipping classes, separated by commas. Each entry consists of a CODE;Description pair, separated by a semi-colon.','SEA;Sea Mail, AIR;Air Mail, ECI_M;Express Courier International, EPI;Express Post International');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.AusPost.MaxWeight')
        INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.AusPost.MaxWeight','RTSHIPPING','The maximum allowed weight for an Australia Post shipment, in kg. If an order weight exceeds this, then the AppConfig:CallForShippingPrompt will be displayed as the shipping method, with a $0 cost.','20');
    go

    if not exists(select * from dbo.AppConfig where name = 'RTShipping.UPS.AccountNumber')
        INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.UPS.AccountNumber','RTSHIPPING','UPS2 Carrier Only. For accounts with negotiated rates, specify the 6-character UPS account.','');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.UPS.AddressTypeBehavior')
        INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.UPS.AddressTypeBehavior','RTSHIPPING','UPS2 Carrier Only. Controls behavior of AddressType indicator in ShipTo addresses. Default is blank, which treats Unknown types as Residential. Legal values are ForceAllResidential, UnknownsAreCommercial, or ForceAllCommercial.','');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.UPS.DeliveryConfirmation')
        INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.UPS.DeliveryConfirmation','RTSHIPPING','UPS2 Carrier Only. Legal values are DeliveryConfirmation, SignatureRequired, or AdultSignatureRequired','');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.UPS.GetNegotiatedRates')
        INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.UPS.GetNegotiatedRates','RTSHIPPING','UPS2 Carrier Only. When true, retrieves negotiated rates for the account specified in RTShipping.UPS.AccountNumber.','false');
    go
    if not exists(select * from dbo.AppConfig where name = 'RTShipping.UPS.Services')
        INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'RTShipping.UPS.Services','RTSHIPPING','UPS2 Carrier Only. A comma-separated list of UPS services, in the format CODE;DisplayName.','03;UPS Ground,12;UPS Three-Day Select,02;UPS Second Day Air,13;UPS Next Day Air Saver,01;UPS Next Day Air,14;UPS Next Day Air Early AM,07;UPS Worldwide Express,08;UPS Worldwide Expedited,11;UPS Standard,54;UPS Worldwide Express Plus,59;UPS Second Day Air AM,65;UPS Saver');
    go

    if not exists(select * from dbo.AppConfig where name = 'RTShipping.SortByRate')
      INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'RTShipping.SortByRate','RTSHIPPING','To sort the list of rates by increasing rate, set to true.','false');
    go





    if not exists(select * from dbo.AppConfig where name = 'TaxCalcMode')
        INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'TaxCalcMode','TAX','Determines if tax calculation is based on shipping address or billing address.  Allowed values are "shipping" and "billing"','shipping');
    go



    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.country')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.country','GATEWAY','eSelectPlus API country code. Either US or CA','US');
    go
    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.URL.CA.Live')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.URL.CA.Live','GATEWAY','eSelectPlus XML API URL for Live mode CA','https://www3.moneris.com/gateway2/servlet/MpgRequest');
    go
    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.URL.CA.LiveMPI')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.URL.CA.LiveMPI','GATEWAY','eSelectPlus MPI API URL for Live mode CA','https://www3.moneris.com/mpi/servlet/MpiServlet');
    go
    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.URL.CA.Test')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.URL.CA.Test','GATEWAY','eSelectPlus XML API URL for Test mode CA','https://esqa.moneris.com/gateway2/servlet/MpgRequest');
    go
    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.URL.CA.TestMPI')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.URL.CA.TestMPI','GATEWAY','eSelectPlus MPI API URL for Test mode CA','https://esqa.moneris.com/mpi/servlet/MpiServlet');
    go
    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.URL.US.Live')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.URL.US.Live','GATEWAY','eSelectPlus XML API URL for Live mode US','https://esplus.moneris.com/gateway_us/servlet/MpgRequest');
    go
    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.URL.US.LiveMPI')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.URL.US.LiveMPI','GATEWAY','eSelectPlus MPI API URL for Live mode US','https://esplus.moneris.com/mpi/servlet/MpiServlet');
    go
    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.URL.US.Test')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.URL.US.Test','GATEWAY','eSelectPlus XML API URL for Test mode US','https://esplusqa.moneris.com/gateway_us/servlet/MpgRequest');
    go
    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.URL.US.TestMPI')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.URL.US.TestMPI','GATEWAY','eSelectPlus MPI API URL for Test mode US','https://esplusqa.moneris.com/mpi/servlet/MpiServlet');
    go
    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.api_token.Test')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.api_token.Test','GATEWAY','eSelectPlus API Token for Test mode','qatoken');
    go
    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.store_id.Test')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.store_id.Test','GATEWAY','eSelectPlus Store ID for Test mode','monusqa002');
    go
    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.api_token.Live')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.api_token.Live','GATEWAY','eSelectPlus API Token for Live mode','');
    go
    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.store_id.Live')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.store_id.Live','GATEWAY','eSelectPlus Store ID for Live mode','');
    go
    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.crypt')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.crypt','GATEWAY','eSelectPlus Default Crypt Value','7');
    go
    if not exists(select * from dbo.AppConfig where name = 'eSelectPlus.includeAVS')
     INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'eSelectPlus.includeAVS','GATEWAY','eSelectPlus control of sending AVS data to gateway. True sends AVS, False does not send AVS.','true');
    go


    if not exists(select * from dbo.AppConfig where name = 'BuySafe.Enabled')
        INSERT INTO AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'BuySafe.Enabled', 'buySAFE','This flag enables buySAFE features once you have signed up with buySAFE. Refer to http://www.buysafe.com for more information.', 'false')
    go

    if not exists(select * from dbo.AppConfig where name = 'BuySafe.Username')
        INSERT INTO AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) VALUES(1,'BuySafe.Username', 'buySAFE', 'MSP username from buySAFE. Do NOT change unless you are instructed, as this may invalidate your buySAFE features if you are using them', 'aspdotnetstorefront')
    go

    if not exists(select * from dbo.AppConfig where name = 'BuySafe.Password')
        INSERT INTO AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) VALUES(1,'BuySafe.Password', 'buySAFE', 'MSP password from buySAFE.Do NOT change unless you are instructed, as this may invalidate your buySAFE features if you are using them', '96736ADD-ECC8-4FD2-9525-9FD9449CC95F')
    go

    if not exists(select * from dbo.AppConfig where name = 'BuySafe.MspId')
        INSERT INTO AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'BuySafe.MspId', 'buySAFE', 'MSP identifier from buySAFE. Do NOT change unless you are instructed, as this may invalidate your buySAFE features if you are using them', '8')
    go

    if not exists(select * from dbo.AppConfig where name = 'BuySafe.Hash')
        INSERT INTO AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'BuySafe.Hash', 'buySAFE', 'Enter your buySAFE assigned publich hash code here, This value is provided to you when you sign up for buySAFE', '')
    go

    if not exists(select * from dbo.AppConfig where name = 'BuySafe.StoreToken')
        INSERT INTO AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'BuySafe.StoreToken', 'buySAFE', 'Enter your Store Token assigned to you by buySAFE. This value is provided to you when you sign up for buySAFE', '')
    go

    if not exists(select * from dbo.AppConfig where name = 'BuySafe.JsUrl')
        INSERT INTO AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'BuySafe.JsUrl', 'buySAFE', 'URL to buySAFE''s javascript. Do NOT change unless you are instructed, as this may invalidate your buySAFE features if you are using them', 'https://seal.buysafe.com/private/rollover/rollover.js')
    go

    if not exists(select * from dbo.AppConfig where name = 'BuySafe.RegUrl')
        INSERT INTO AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'BuySafe.RegUrl', 'buySAFE', 'The buySAFE registration page URL. Do NOT change unless you are instructed, as this may invalidate your buySAFE features if you are using them', 'https://www.buysafe.com/web/login/registrationoptions.aspx')
    go

    if not exists(select * from dbo.AppConfig where name = 'BuySafe.InvitationCode')
        INSERT INTO AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'BuySafe.InvitationCode', 'buySAFE', 'AspDotNetStorefront reg code. Do NOT change unless you are instructed, as this may invalidate your buySAFE features if you are using them', 'offerASPDOTNET')
    go

    if not exists(select * from dbo.AppConfig where name = 'BuySafe.SealSize')
        INSERT INTO AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'BuySafe.SealSize', 'buySAFE', 'Specifies the size of the buySAFE seal graphic used, when buySAFE is enabled. Allowed values are: Small, Medium, Large, and Persistent. To use a small seal, set this value to small. The size of seal you should use is dependent on the structure of your skin, and where you have this seal appear', 'Small')
    go



    if not exists(select * from dbo.AppConfig where name = 'HSBC.CcpaURL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'HSBC.CcpaURL','GATEWAY','HSBC URL for client form post when using Payer Authentication.','https://www.ccpa.hsbc.com/ccpa');
    go
    if not exists(select * from dbo.AppConfig where name = 'HSBC.CcpaClientID')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'HSBC.CcpaClientID','GATEWAY','HSBC merchant identifier for Payer Authentication. Typically of the format UK12345678CUR01 (15 characters). See also AppConfig 3DSecure.CreditCardTypeIDs.','');
    go


    if not exists(select * from dbo.AppConfig where name = 'HSBC.CcpaURL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'HSBC.CcpaURL','GATEWAY','HSBC URL for client form post when using Payer Authentication.','https://www.ccpa.hsbc.com/ccpa');
    go
    if not exists(select * from dbo.AppConfig where name = 'HSBC.CcpaClientID')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'HSBC.CcpaClientID','GATEWAY','HSBC merchant identifier for Payer Authentication. Typically of the format UK12345678CUR01 (15 characters). See also AppConfig 3DSecure.CreditCardTypeIDs.','');
    go


    if not exists(select * from dbo.AppConfig where name = 'ShowEditButtonInCartForKitProducts')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ShowEditButtonInCartForKitProducts','MISC','If true, an edit button will be shown in the shopping cart next to kit products, allowing customers to edit/change them','true');
    go
    if not exists(select * from dbo.AppConfig where name = 'ShowEditButtonInCartForPackProducts')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ShowEditButtonInCartForPackProducts','MISC','If true, an edit button will be shown in the shopping cart next to pack products, allowing customers to edit/change them','true');
    go
    if not exists(select * from dbo.AppConfig where name = 'ShowEditButtonInCartForRegularProducts')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'ShowEditButtonInCartForRegularProducts','MISC','If true, an edit button will be shown in the shopping cart next to regular (non kit or pack) products, allowing customers to edit/change them','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'MultiImage.UseProductIconPics')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'MultiImage.UseProductIconPics','IMAGERESIZE','','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'MultiImage.UseProductIconPics')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'MultiImage.UseProductIconPics','IMAGERESIZE','','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'Zoomify.Active')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Zoomify.Active','SITEDISPLAY','Enable display of Zoomify Viewer when Zoomify image content is available.','false');
    go
    if not exists(select * from dbo.AppConfig where name = 'Zoomify.GalleryMedium')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Zoomify.GalleryMedium','SITEDISPLAY','Enable Zoomify content for medium product gallery images using the standard LookupProductImage function within XmlPackages.','true');
    go
    if not exists(select * from dbo.AppConfig where name = 'Zoomify.Large.Height')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Zoomify.Large.Height','SITEDISPLAY','Large Zoomify Viewer Height','500');
    go
    if not exists(select * from dbo.AppConfig where name = 'Zoomify.Large.Width')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Zoomify.Large.Width','SITEDISPLAY','Large Zoomify Viewer Width','500');
    go
    if not exists(select * from dbo.AppConfig where name = 'Zoomify.Medium.Height')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Zoomify.Medium.Height','SITEDISPLAY','Medium Zoomify Viewer Height','250');
    go
    if not exists(select * from dbo.AppConfig where name = 'Zoomify.Medium.Width')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Zoomify.Medium.Width','SITEDISPLAY','Medium Zoomify Viewer Width','250');
    go
    if not exists(select * from dbo.AppConfig where name = 'Zoomify.ProductLarge')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Zoomify.ProductLarge','SITEDISPLAY','Enable Zoomify content for large product images using the standard LookupProductImage function within XmlPackages.','true');
    go
    if not exists(select * from dbo.AppConfig where name = 'Zoomify.ProductMedium')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Zoomify.ProductMedium','SITEDISPLAY','Enable Zoomify content for medium product images using the standard LookupProductImage function within XmlPackages.','true');
    go

    if not exists(select * from dbo.AppConfig where name = 'CardinalCommerce.Centinel.TransactionPwd')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'CardinalCommerce.Centinel.TransactionPwd','CARDINAL','Your Cardinal Centinal Assigned Transaction Password.','');
    go
    if not exists(select * from dbo.AppConfig where name = 'CardinalCommerce.Centinel.MyECheckMarkAsCaptured')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'CardinalCommerce.Centinel.MyECheckMarkAsCaptured','CARDINAL','When false the order must be marked as Captured manually upon review and approval in the store admin. If true then new orders with Cardinal MyECheck will immediately be marked as Captured. This should only be set to true if you are subscribed with the MyECheck payment guarantee service.','false');
    go


    if not exists(select * from dbo.AppConfig where name = 'Shipwire.Username')
        INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'Shipwire.Username','SHIPPING','Your Shipwire Username, provided by Shipwire. Typically the email address you used when you signed-up. ','');
    go

    if not exists(select * from dbo.AppConfig where name = 'Shipwire.Password')
        INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (0,'Shipwire.Password','SHIPPING','Your Shipwire Password, provided by Shipwire.','');
    go



    if not exists(select * from dbo.AppConfig where name = 'AuditLog.Enabled')
        INSERT [dbo].AppConfig (SuperOnly,Name,GroupName,Description,ConfigValue) values (1,'AuditLog.Enabled','MISC','Set to true to log actions affecting a customer order. The log is viewable by following the link on the Customer History page. (Currently only Recurring Orders are implemented.)','false');
    go

    if not exists(select * from dbo.AppConfig where name = 'Admin_OrderStatisticIsChart')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Admin_OrderStatisticIsChart','ADMIN','if you want a chart type order statistics, set this to true. Otherwise order statistics will be displayed as a table','true');
    go

    if not exists(select * from dbo.AppConfig where name = 'CacheEntityPageHTML')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0, 'CacheEntityPageHTML', 'GENERAL', 'if true, this enables HTML caching of your category, section (department) and manufacturer pages. You can use this setting in most cases, as long as you are not doing real time inventory updates (to the minute) or other specific entity page product filtering', 'false')
    go

    if not exists(select * from dbo.AppConfig where name = 'NumPreviouslyUsedPwds')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1, 'NumPreviouslyUsedPwds', 'SECURITY', 'Prevents admin users from re-using any of the specified number of previously used passowrds, the PABP requirement is 4', '4')
    go

    if not exists(select * from dbo.AppConfig where name = 'Google.EcomOrderTrackingEnabled')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Google.EcomOrderTrackingEnabled','MISC','Determines whether the google ecommerce tracking version 2 code is fired on the order confirmation.  If this AppConfig is disabled, the GOOGLE_ECOM_TRACKING_V2 token will still function, however order details will not be sent to google.','false');
    go

    if not exists(select * from [dbo].AppConfig WHERE Name='Google.AnalyticsAccount')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Google.AnalyticsAccount','MISC','The google tracking account to be used with the google analytics tracking javascript code (ga.js).  This does not apply to the urchin tracking javascript code (urchin.js).','');
    go

    if not exists(select * from dbo.AppConfig where name = 'ShowShippingAndTaxEstimate')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'ShowShippingAndTaxEstimate','SHIPPING','If ShowShippingAndTaxEstimate is set to true, the Shipping and Tax Estimator control will show in ShoppingCart pages','true');
    go

    if not exists(select * from dbo.AppConfig where name = 'DisallowAnonCustomerToCreateWishlist')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'DisallowAnonCustomerToCreateWishlist','ADMIN','if set to true, and the customer is anonymous, it will redirect the customer to signin page to and force login before it can add to wishlist.','false');
    go


    -- For 404.ComparisonDistance
    if not exists(select * from dbo.AppConfig where name = '404.ComparisonDistance')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'404.ComparisonDistance','404','By defualt .60, this value filter the likeness of words. The higher the value, the more different the string is.   Suggested value .60 to .70.','.60');
    go

    -- 404.VisibleSuggestions
       if not exists(select * from dbo.AppConfig where name = '404.VisibleSuggestions')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'404.VisibleSuggestions','404','By default the value is blank, it will suggest valid store site pages related to product, category, manufacturer, section, and topic (eg. of suggested pages ShoppingCart.aspx,p-3-simple-product-3.aspx,m-1-generic-mfg.aspx).You can filter the suggested pages by comma seperated list(eg. category,topic).Valid values are product, category, manufacturer, section, topic','');
    go

    -- Show404SuggestionLinks
    if not exists(select * from dbo.AppConfig where name = 'Show404SuggestionLinks')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'Show404SuggestionLinks','404','If Show404SuggestionLinks is set to true, suggestion of links will be enable and visible whenever redirect to 404.aspx','true');
    go

    -- 404.NumberOfSuggestedLinks
    if not exists(select * from dbo.AppConfig where name = '404.NumberOfSuggestedLinks')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'404.NumberOfSuggestedLinks','404','This appconfig set the limit of the suggested links. The default value is 5 ,it will show up to 5 suggested links.','5');
    go

	-- Also Bought
	if not exists(select * from dbo.AppConfig where name = 'AlsoBoughtNumberToDisplay')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'AlsoBoughtNumberToDisplay','XMLPACKAGE','controlS the maximum number of AlsoBought products that will be displayed at the bottom of the Product pages',4);
    go

	if not exists(select * from dbo.AppConfig where name = 'AlsoBoughtProductsFormat')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'AlsoBoughtProductsFormat','XMLPACKAGE','Controls how AlsoBought Products are displayed at the bottom of Product pages. Valid values are GRID or TABLE. Grid uses AppConfig:AlsoBoughtProductsGridColWidth as # items per each row.','GRID');
    go

	if not exists(select * from dbo.AppConfig where name = 'AlsoBoughtProductsGridColWidth')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'AlsoBoughtProductsGridColWidth','XMLPACKAGE','if using grid layout for AlsoBought products, this is the # of cols in the grid',4);
    go


    if not exists(select * from dbo.AppConfig where name = 'DisplayOutOfStockProducts')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DisplayOutOfStockProducts','OUTOFSTOCK','This will show ''out of stock'' or ''In stock'' message depending on the inventory of product. If the inventory is less than OutOfStockThreshold value, it will display ''Out of stock'' message, otherwise ''In stock'' message. This can enable by setting the value to true and will automatically disable the HideProductsWithLessThanThisInventoryLevel. You can totaly configure it in Misc -> Inventory Control page.','False');
    GO

    if not exists(select * from dbo.AppConfig where name = 'DisplayOutOfStockOnProductPages')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DisplayOutOfStockOnProductPages','OUTOFSTOCK','This will show ''out of stock'' or ''In stock'' message on product pages only. To enable this you must first set DisplayOutOfStockProducts to true','False');
    GO

    if not exists(select * from dbo.AppConfig where name = 'DisplayOutOfStockOnEntityPages')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DisplayOutOfStockOnEntityPages','OUTOFSTOCK','This will show ''out of stock'' or ''In stock'' message on entity pages only. To enable this you must first set DisplayOutOfStockProducts to true','False');
    GO

    if not exists(select * from dbo.AppConfig where name = 'OutOfStockThreshold')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'OutOfStockThreshold','OUTOFSTOCK','This is the flag use in DisplayOutOfStockProducts. To enable this you must first set DisplayOutOfStockProducts to true. Valid value 0,1,2,7,100.','0');
    GO

    if not exists(select * from dbo.AppConfig where name = 'SagePayments.MERCHANT_KEY')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'SagePayments.MERCHANT_KEY','GATEWAY','Transaction key provided by SagePayments','');
    GO


    if not exists(select * from dbo.AppConfig where name = 'SagePayments.ServiceURLTEST')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'SagePayments.ServiceURLTEST','GATEWAY','Your SagePayments assigned gateway test mode posting URL','https://www.sagepayments.net/web_services/vterm_extensions/transaction_processing.asmx?WSDL');
    GO

    if not exists(select * from dbo.AppConfig where name = 'SagePayments.ServiceURL')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'SagePayments.ServiceURL','GATEWAY','Your SagePayments assigned gateway URL','https://www.sagepayments.net/web_services/vterm_extensions/transaction_processing.asmx?WSDL');
    GO

    if not exists(select * from dbo.AppConfig where name = 'SagePayments.MERCHANT_ID')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'SagePayments.MERCHANT_ID','GATEWAY','Your Merchant id, assigned by SagePayments.','');
    GO




    if not exists(select * from dbo.AppConfig where name = 'DynamicRelatedProducts.Enabled')
        insert dbo.AppConfig(SuperOnly, Name, GroupName, Description, ConfigValue) values(0, 'DynamicRelatedProducts.Enabled','DISPLAY','When set to true, the products that are related to the product currently viewed by the customer will be displayed at the bottom of the Product page. Valid values are true and false only','false')
    GO

    if not exists(select * from dbo.AppConfig where name = 'DynamicRelatedProducts.NumberDisplayed')
        insert dbo.AppConfig(SuperOnly, Name, GroupName, Description, ConfigValue) values(0, 'DynamicRelatedProducts.NumberDisplayed','DISPLAY','Controls the maximum number of related products that will be displayed at the bottom of the Product pages. Valid values ranges from 1-10','4')
    GO


    if not exists(select * from dbo.AppConfig where name = 'RecentlyViewedProducts.Enabled')
        insert dbo.Appconfig(SuperOnly, Name, GroupName, Description, ConfigValue) values(0, 'RecentlyViewedProducts.Enabled', 'DISPLAY', 'When set to true, the products that were viewed previoulsy by the customer will be displayed at the bottom of the Product page. Valid values are true and false only.', 'false')
    GO

    if not exists(select * from dbo.AppConfig where name = 'RecentlyViewedProducts.NumberDisplayed')
        insert dbo.Appconfig(SuperOnly, Name, GroupName, Description, ConfigValue) values(0, 'RecentlyViewedProducts.NumberDisplayed', 'DISPLAY', 'Controls the maximum number of Recently Viewed products that will be displayed at the bottom of the Product page. Valid values are from 1-10.', '4')
    GO

    if not exists(select * from dbo.AppConfig where name = 'RecentlyViewedProducts.ProductsFormat')
        insert dbo.Appconfig(SuperOnly, Name, GroupName, Description, ConfigValue) values(0, 'RecentlyViewedProducts.ProductsFormat', 'XMLPACKAGE', 'Controls how Recently Viewed Products are displayed at the bottom of Product page. Valid values are GRID or TABLE. Grid uses AppConfig:RecentlyViewedProductsGridColWidth as # items per each row.', 'GRID')  
    GO

    if not exists(select * from dbo.AppConfig where name = 'RecentlyViewedProductsGridColWidth')
        insert dbo.Appconfig(SuperOnly, Name, GroupName, Description, ConfigValue) values(0, 'RecentlyViewedProductsGridColWidth', 'XMLPACKAGE' , 'If product format is GRID, this is the # of columns in the grid. Valid values are non-negative or greater than or equal to 1.', '4')  
    GO

    if not exists(select * from dbo.AppConfig where name = 'PaymentExpress.Username')
        insert dbo.Appconfig(SuperOnly, Name, GroupName, Description, ConfigValue) values(1, 'PaymentExpress.Username', 'GATEWAY' , 'Username assigned by Payment Express', '')  
    GO

    if not exists(select * from dbo.AppConfig where name = 'PaymentExpress.Password')
        insert dbo.Appconfig(SuperOnly, Name, GroupName, Description, ConfigValue) values(1, 'PaymentExpress.Password', 'GATEWAY' , 'Password assigned by Payment Express', '')  
    GO

    if not exists(select * from dbo.AppConfig where name = 'Captcha.NumberOfCharacters')    
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Captcha.NumberOfCharacters', 'SECURITY', 'Specifies the number of characters that the Captcha generator will use.  The minimum number of characters that can be used is 6.', '6')
    GO

    if not exists(select * from dbo.AppConfig where name = 'Captcha.AllowedCharactersRegex')    
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Captcha.AllowedCharactersRegex', 'SECURITY', 'Regex value for the allowed characters in the image.  See the AspDotNetStorefront manual for additional regex values.  Do not modify this value unless you are sure of what you are doing.', '[a-zA-Z0-9]')
    GO

    if not exists(select * from dbo.AppConfig where name = 'Captcha.ImageBackColor')    
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Captcha.ImageBackColor', 'SECURITY', 'The color (Red, Blue, Green, etc...) or hex value (#0000FF) for the back color of the captcha image.', 'LightGray')
    GO

    if not exists(select * from dbo.AppConfig where name = 'Captcha.ImageForeColor')   
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Captcha.ImageForeColor', 'SECURITY', 'The color (Red, Blue, Green, etc...) or hex value (#0000FF) for the fore color of the captcha image.', 'White') 
    GO

    if not exists(select * from dbo.AppConfig where name = 'Captcha.TextBackColor')  
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Captcha.TextBackColor', 'SECURITY', 'The color (Red, Blue, Green, etc...) or hex value (#0000FF) for the back color of the captcha text.', 'DarkGray')  
    GO

    if not exists(select * from dbo.AppConfig where name = 'Captcha.TextForeColor')    
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Captcha.TextForeColor', 'SECURITY', 'The color (Red, Blue, Green, etc...) or hex value (#0000FF) for the fore color of the captcha text.', 'Black')
    GO

    if not exists(select * from dbo.AppConfig where name = 'Captcha.HorizontalColor')    
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Captcha.HorizontalColor', 'SECURITY', 'The color (Red, Blue, Green, etc...) or hex value (#0000FF) for the horizontal colored lines.', 'Blue')
    GO

    if not exists(select * from dbo.AppConfig where name = 'Captcha.VerticalColor')    
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Captcha.VerticalColor', 'SECURITY', 'The color (Red, Blue, Green, etc...) or hex value (#0000FF) for the horizontal colored lines.', 'Blue')
    GO

    if not exists(select * from dbo.AppConfig where name = 'Captcha.CaseSensitive')    
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'Captcha.CaseSensitive', 'SECURITY', 'If true, the captcha image will be case-sensitive.', 'true')
    GO

    if not exists(select * from dbo.AppConfig where name = 'Captcha.MaxAsciiValue')    
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,Hidden) values(1,'Captcha.MaxAsciiValue', 'SECURITY', 'Maximum ASCII value used in Captcha random character generation.  Do not modify this value unless you know what you are doing.', '126', 1)
    GO

    if not exists(select * from dbo.AppConfig where name = 'NETAXEPT.Test_UI')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'NETAXEPT.Test_UI','GATEWAY','The test url for BBS Hosted UI,do not change this value unless you know what you are doing','https://epay.bbs.no:9443/cgi/epay.pway');
    GO

    if not exists(select * from dbo.AppConfig where name = 'NETAXEPT.Live_UI')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'NETAXEPT.Live_UI','GATEWAY','The production url for BBS Hosted UI,do not change this value unless you know what you are doing','https://epay.bbs.no:443/cgi/epay.pway');
    GO

    if not exists(select * from dbo.AppConfig where name = 'NETAXEPT.Test_Server')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'NETAXEPT.Test_Server','GATEWAY','The test server url ,do not change this value unless you know what you are doing','https://epayment-test.bbs.no/service.svc?wsdl');
    GO

    if not exists(select * from dbo.AppConfig where name = 'NETAXEPT.Live_Server')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'NETAXEPT.Live_Server','GATEWAY','The production server,do not change this value unless you know what you are doing','https://epayment.bbs.no/service.svc?wsdl');
    GO

    if not exists(select * from dbo.AppConfig where name = 'NETAXEPT.Merchant_Id')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'NETAXEPT.Merchant_Id','GATEWAY','The merchant ID is the ID given to you by BBS.','');
    GO

    if not exists(select * from dbo.AppConfig where name = 'NETAXEPT.Merchant_Token')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'NETAXEPT.Merchant_Token','GATEWAY','The token is the shared secret between merchant and BBS. This is similar to a password.','');
    GO

    if not exists(select * from dbo.AppConfig where name = 'NETAXEPT.MerchantSettings.RedirectUrl')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'NETAXEPT.MerchantSettings.RedirectUrl','GATEWAY','After payment information at BBS, customer will be redirected to this page.','netaxept.aspx');
    GO

    if not exists(select * from dbo.AppConfig where name = 'NETAXEPT.Error.Setup')
        INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'NETAXEPT.Error.Setup','GATEWAY','If true, an error message caused by the setup call will be shown in the checkoutpayment page, otherwise it will be redirected to the BBS UI interface','true');
    GO











    update dbo.AppConfig set Description = 'Your UPS Account UserName, assigned by UPS.' where Name = 'RTShipping.UPS.UserName'
    update dbo.AppConfig set Description = 'Your UPS Account Password, assigned by UPS.' where Name = 'RTShipping.UPS.Password'
    update dbo.AppConfig set Description = 'Your UPS Account License, or XML Access Key they may call this.' where Name = 'RTShipping.UPS.License'


    update dbo.AppConfig set ConfigValue = 'https://onlinetools.ups.com/ups.app/xml/Rate' where Name = 'RTShipping.UPS.Server';


    update dbo.AppConfig set ConfigValue = 'false', Description = 'This flag enables buySAFE features once you have signed up with buySAFE. Refer to http://www.buysafe.com for more information.' where Name = 'BuySafe.Enabled'
    update dbo.AppConfig set ConfigValue = 'aspdotnetstorefront', Description = 'MSP username from buySAFE. Do NOT change unless you are instructed, as this may invalidate your buySAFE features if you are using them' where Name = 'BuySafe.Username'
    update dbo.AppConfig set ConfigValue = '96736ADD-ECC8-4FD2-9525-9FD9449CC95F', Description = 'MSP password from buySAFE.Do NOT change unless you are instructed, as this may invalidate your buySAFE features if you are using them' where Name = 'BuySafe.Password'
    update dbo.AppConfig set ConfigValue = '8', Description = 'MSP identifier from buySAFE. Do NOT change unless you are instructed, as this may invalidate your buySAFE features if you are using them' where Name = 'BuySafe.MspId'
    update dbo.AppConfig set ConfigValue = '', Description = 'Enter your buySAFE assigned publich hash code here, This value is provided to you when you sign up for buySAFE' where Name = 'BuySafe.Hash'
    update dbo.AppConfig set ConfigValue = '', Description = 'Enter your Store Token assigned to you by buySAFE. This value is provided to you when you sign up for buySAFE' where Name = 'BuySafe.StoreToken'
    update dbo.AppConfig set ConfigValue = 'https://seal.buysafe.com/private/rollover/rollover.js', Description = 'URL to buySAFE''s javascript. Do NOT change unless you are instructed, as this may invalidate your buySAFE features if you are using them'where Name = 'BuySafe.JsUrl'
    update dbo.AppConfig set ConfigValue = 'https://www.buysafe.com/web/login/registrationoptions.aspx', Description = 'The buySAFE registration page URL. Do NOT change unless you are instructed, as this may invalidate your buySAFE features if you are using them' where Name = 'BuySafe.RegUrl'
    update dbo.AppConfig set ConfigValue = 'offerASPDOTNET', Description = 'AspDotNetStorefront reg code. Do NOT change unless you are instructed, as this may invalidate your buySAFE features if you are using them' where Name = 'BuySafe.InvitationCode'



    update dbo.AppConfig set Description = 'Enter the Name of the quantity discount table (that you have previouslyCREATEd) that you want to be active for ALL ProductS on the site. If you enter a discount table Name here, it applies to all Products. If a category has a discount table also specified, the category discount table overrides this one. If a Product has a discount table specified, the Product discount table overrides the category and the site one. So the order in which Discount Tables are applied to a product are in order (until one is found): 1. Product Level, 2. Category Level, and 3. Site Level (specified by this app config parm). Leave this parameter blank if you don''t want any discount table to be applied to all Products.' where name = 'ActiveQuantityDiscountTable'
    go



    if exists(select * from dbo.AppConfig where name = 'eSelectPlus.store_id') begin
        update dbo.AppConfig set ConfigValue = (select ConfigValue from dbo.AppConfig where name = 'eSelectPlus.store_id') where name = 'eSelectPlus.store_id.Live'
        update dbo.AppConfig set ConfigValue = (select ConfigValue from dbo.AppConfig where name = 'eSelectPlus.store_id') where name = 'eSelectPlus.store_id.Test'
        update dbo.AppConfig set ConfigValue = (select ConfigValue from dbo.AppConfig where name = 'eSelectPlus.api_token') where name = 'eSelectPlus.api_token.Live'
        update dbo.AppConfig set ConfigValue = (select ConfigValue from dbo.AppConfig where name = 'eSelectPlus.api_token') where name = 'eSelectPlus.api_token.Test'

        update dbo.AppConfig set Description = 'eSelectPlus Default Crypt Value' where name = 'eSelectPlus.crypt'

        if (select charindex('plus', lower(ConfigValue)) from dbo.AppConfig where name = 'eSelectPlus.host') = 0
           -- we are not in US so set to CA
           update dbo.AppConfig set ConfigValue = 'CA' where name = 'eSelectPlus.country'

        delete from dbo.AppConfig where name = 'eSelectPlus.commcard_invoice'
        delete from dbo.AppConfig where name = 'eSelectPlus.commcard_tax_OrderTotal'
        delete from dbo.AppConfig where name = 'eSelectPlus.host'
        delete from dbo.AppConfig where name = 'eSelectPlus.api_token'
        delete from dbo.AppConfig where name = 'eSelectPlus.store_id'
    end
    go



    -- Updated URLs for GoogleCheckout.BaseUrl and GoogleCheckout.SandBoxCheckoutURL
    UPDATE [dbo].AppConfig SET ConfigValue = 'https://checkout.google.com/api/checkout/v2/request/Merchant/{0}', [Description] = 'Don''t change this value. It''s the URL that your website uses to communicate with Google Checkout. The replace token in the string is for the merchantid.' where Name = 'GoogleCheckout.BaseUrl'
    UPDATE [dbo].AppConfig SET ConfigValue = 'https://sandbox.google.com/checkout/api/checkout/v2/request/Merchant/{0}' where Name = 'GoogleCheckout.SandBoxCheckoutURL'
    go

    update [dbo].AppConfig set ConfigValue='https://webservices.primerchants.com/billing/TransactionCentral/processCC.asp' where Name='TRANSACTIONCENTRAL_LIVE_SERVER'
    update [dbo].AppConfig set ConfigValue='https://webservices.primerchants.com/billing/TransactionCentral/processCC.asp' where Name='TRANSACTIONCENTRAL_TEST_SERVER'
    update [dbo].AppConfig set ConfigValue='https://webservices.primerchants.com/billing/TransactionCentral/voidcreditcconline.asp' where Name='TRANSACTIONCENTRAL_VOID_SERVER'

    update [dbo].appconfig set Description='The cell messaging number of your phone, in the EXACT format required by your carrier to receive a message, e.g. xxxxxxxxx@mmode.com, or whatever. Contact your cell carrier for more instructions on how to send SMS messages to your phone.' where name='Dispatch_ToPhoneNumber';

    update [dbo].AppConfig set GroupName='XMLPACKAGE', Name='XmlPackage.IncludeCookies' where Name='Xml.IncludeCookies';
    update [dbo].AppConfig set GroupName='XMLPACKAGE', Name='XmlPackage.DumpTransform' where Name='Xml.DumpTransform';

    update dbo.AppConfig set ConfigValue='https://api.sandbox.paypal.com/2.0/' where Name='PayPalPro.TestURL';
    update dbo.AppConfig set ConfigValue='https://api-3t.paypal.com/2.0/' where Name='PayPalPro.LiveURL';
    update dbo.AppConfig set ConfigValue='https://api-aa.sandbox.paypal.com/2.0/' where Name='PayPalPro.TestAAURL';
    update dbo.AppConfig set ConfigValue='https://api-aa-3t.paypal.com/2.0/' where Name='PayPalPro.LiveAAURL';
    -- set default properly on Cardinal Commerce live url (fixed in main db script also):
    update dbo.appconfig set ConfigValue='https://centinel800.cardinalcommerce.com/maps/txns.asp' where Name='CardinalCommerce.Centinel.TransactionUrl.Live' and ConfigValue like '%test%';

    update dbo.AppConfig set Description='if you want manufacturers to be shown in the sitemap, set this to true.' where [name]='SiteMap.ShowManufacturers'


    update dbo.AppConfig set groupname = 'GENERAL' where groupname is null
    go

    update dbo.AppConfig set Description='This value specifies what credit card payment processing gateway you are using. To set your active Payment Gateway, run the Configuration Wizard page in the admin site.' where Name='PaymentGateway';
    go

    update dbo.AppConfig set ConfigValue = 'http://Production.ShippingAPIs.com/ShippingAPI.dll' where name = 'RTShipping.USPS.Server'
    go

    update dbo.AppConfig set ConfigValue='7.0.2.5' where name='StoreVersion';
    go


    update [dbo].Genre set XmlPackage='entity.grid.xml.config' where XmlPackage IS NULL or XmlPackage='';
    go

    update [dbo].Vector set XmlPackage='entity.grid.xml.config' where XmlPackage IS NULL or XmlPackage='';
    go




    if not exists(select * from dbo.AppConfig where name = 'PayPal.BusinessID')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.BusinessID' where name = 'PayPal_Business'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.LiveServer')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.LiveServer' where name = 'PayPal_Live_Server'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.ReturnCancelURL')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.ReturnCancelURL' where name = 'PayPal_ReturnCancelURL'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.ReturnOKURL')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.ReturnOKURL' where name = 'PayPal_ReturnOKURL'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.API.Username')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.API.Username' where name = 'PayPalPro.Username'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.API.Password')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.API.Password' where name = 'PayPalPro.Password'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.API.MerchantEMailAddress')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.API.MerchantEMailAddress' where name = 'PayPalPro.MerchantEMailAddress'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.API.TestURL')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.API.TestURL' where name = 'PayPalPro.TestURL'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.API.LiveURL')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.API.LiveURL' where name = 'PayPalPro.LiveURL'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.API.TestAAURL')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.API.TestAAURL' where name = 'PayPalPro.TestAAURL'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.API.LiveAAURL')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.API.LiveAAURL' where name = 'PayPalPro.LiveAAURL'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.API.Version')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.API.Version' where name = 'PayPalPro.Version'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.API.RefundVersion')
    UPDATE [dbo].AppConfig SET Name = 'PayPal.API.RefundVersion' where name = 'PayPalPro.RefundVersion'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.UseInstantNotification')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.UseInstantNotification' where name = 'PayPal_UseInstantNotification'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.NotificationURL')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.NotificationURL' where name = 'PayPal_NotificationURL'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.Express.CancelURL')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.Express.CancelURL' where name = 'PayPalExpress.CancelURL'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.Express.LiveURL')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.Express.LiveURL' where name = 'PayPalExpress.LiveURL'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.Express.ReturnURL')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.Express.ReturnURL' where name = 'PayPalExpress.ReturnURL'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.Express.SandboxURL')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.Express.SandboxURL' where name = 'PayPalExpress.SandboxURL'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.Express.PageStyle')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.Express.PageStyle' where name = 'PayPalExpress.PageStyle'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.Express.HeaderImage')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.Express.HeaderImage' where name = 'PayPalExpress.HeaderImage'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.Express.HeaderBackColor')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.Express.HeaderBackColor' where name = 'PayPalExpress.HeaderBackColor'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.Express.HeaderBorderColor')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.Express.HeaderBorderColor' where name = 'PayPalExpress.HeaderBorderColor'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.Express.PayFlowColor')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.Express.PayFlowColor' where name = 'PayPalExpress.PayFlowColor'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.API.Signature')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.API.Signature' where name = 'PayPalPro.Signature'
    go

    if not exists(select * from dbo.AppConfig where name = 'PayPal.Express.ShowOnCartPage')
        UPDATE [dbo].AppConfig SET Name = 'PayPal.Express.ShowOnCartPage', ConfigValue = 'true' where name = 'ShowPayPalExpressCheckoutOnCartPage'
    go


    UPDATE [dbo].AppConfig SET Description = 'If you don''t want the PayPal Express Checkout button on your cart page, set this to false. In order to fully enable this payment method you need to select PayPalPro as your payment gateway and of course have a PayPal account with the PayPalPro service enabled. By default, this is set to true and, so they can select the checkout method on our payments page like all other payment methods. The icon will not be shown if the cart is empty also.' where Name = 'PayPal.Express.ShowOnCartPage'

    go

    UPDATE [dbo].AppConfig SET Description = 'Enter the payment methods you want to allow on the store. Valid values are: Credit Card, Purchase Order, eCheck, PayPal, PayPal Express, Request Quote, Check By Mail, and C.O.D. (that is "C.O.D." with the ending period), C.O.D. (Money Order), C.O.D. (Company Check), MICROPAY, C.O.D. (Net 30). You can specify that you want to allow multiple payment methods, just separate them by a comma, e.g. Credit Card, Purchase Order, PayPal' 
    where name = 'PaymentMethods'

    UPDATE [dbo].AppConfig SET Description = 'Test sandbox PayPal SOAP API URL. Do not change this value unless you know what you are doing.' 
    where name = 'PayPal.API.TestURL'

    UPDATE [dbo].AppConfig SET Description = 'Live server PayPal SOAP API URL. Do not change this value unless you know what you are doing.' 
    where name = 'PayPal.API.LiveURL'

    UPDATE [dbo].AppConfig SET Description = 'PayPal Assigned API Username. Consult PayPal Documentation for more information. You get this from the PayPal site.' 
    where name = 'PayPal.API.Username'

    UPDATE [dbo].AppConfig SET Description = 'PayPal Assigned API Password. Consult PayPal Documentation for more information. You get this from the PayPal site.' 
    where name = 'PayPal.API.Password'

    UPDATE [dbo].AppConfig SET Description = 'PayPal Assigned API merchant e-mail address for your account. Consult PayPal Documentation for more information. This is almost ALWAYS left blank!' 
    where name = 'PayPal.API.MerchantEMailAddress'

    UPDATE [dbo].AppConfig SET Description = 'PayPal Assigned API Signature. Consult PayPal Documentation for more information. You get this from the PayPal site.' 
    where name = 'PayPal.API.Signature'


    go


    UPDATE [dbo].AppConfig SET ConfigValue = '' where name = 'CheckoutAnonTeaser'
    go


    -- Verisign_DotNet_LiveURL renamed to PayFlowPro.LiveURL and new value
    if not exists(select * from dbo.AppConfig where name = 'PayFlowPro.LiveURL')
        UPDATE [dbo].AppConfig SET Name = 'PayFlowPro.LiveURL', Description = 'Do not change this unless you know what you are doing.', ConfigValue = 'https://payflowpro.verisign.com/transaction' where name = 'Verisign_DotNet_LiveURL'
    go
    -- Verisign_DotNet_TestURL renamed to PayFlowPro.TestURL and new value
    if not exists(select * from dbo.AppConfig where name = 'PayFlowPro.TestURL')
        UPDATE [dbo].AppConfig SET Name = 'PayFlowPro.TestURL', Description = 'Do not change this unless you know what you are doing.', ConfigValue = 'https://pilot-payflowpro.verisign.com/transaction' where name = 'Verisign_DotNet_TestURL'
    go
    -- Verisign_PARTNER renamed to PayFlowPro.PARTNER
    if not exists(select * from dbo.AppConfig where name = 'PayFlowPro.PARTNER')
        UPDATE [dbo].AppConfig SET Name = 'PayFlowPro.PARTNER', Description = 'If using PayPal PayFlow PRO merchant gateway, this is the partner value assigned by the bank.' where name = 'Verisign_PARTNER'
    go
    -- Verisign_PWD renamed to PayFlowPro.PWD
    if not exists(select * from dbo.AppConfig where name = 'PayFlowPro.PWD')
        UPDATE [dbo].AppConfig SET Name = 'PayFlowPro.PWD', Description = 'If using PayPal PayFlow PRO merchant gateway, this is the password value assigned by the bank.' where name = 'Verisign_PWD'
    go
    -- Verisign_USER renamed to PayFlowPro.USER
    if not exists(select * from dbo.AppConfig where name = 'PayFlowPro.USER')
        UPDATE [dbo].AppConfig SET Name = 'PayFlowPro.USER', Description = 'If using PayPal PayFlow PRO merchant gateway, this is the username value assigned by the bank.' where name = 'Verisign_USER'
    go
    -- Verisign_VENDOR renamed to PayFlowPro.VENDOR
    if not exists(select * from dbo.AppConfig where name = 'PayFlowPro.VENDOR')
        UPDATE [dbo].AppConfig SET Name = 'PayFlowPro.VENDOR', Description = 'If using PayPal PayFlow PRO merchant gateway, this is the vendor value assigned by the bank.' where name = 'Verisign_VENDOR'
    go
    -- Verisign_Verify_Addresses renamed to PayFlowPro.VerifyAddresses
    if not exists(select * from dbo.AppConfig where name = 'PayFlowPro.VerifyAddresses')
        UPDATE [dbo].AppConfig SET Name = 'PayFlowPro.VerifyAddresses', Description = 'If using PayPal PayFlow PRO gateway, this controls if address info is sent to the gateway for verification.' where name = 'Verisign_Verify_Addresses'
    go
    -- If they are currently set to VERISIGN, change it to PAYFLOWPRO
    UPDATE [dbo].AppConfig SET ConfigValue = 'PAYFLOWPRO' where name = 'PaymentGateway' and ConfigValue = 'VERISIGN'
    GO
    -- Change PayflowPro endpoints to paypal.com domain
    UPDATE [dbo].AppConfig SET ConfigValue = 'https://payflowpro.paypal.com' where name = 'PayFlowPro.LiveURL'
    UPDATE [dbo].AppConfig SET ConfigValue = 'https://pilot-payflowpro.paypal.com' where name = 'PayFlowPro.TestURL'
    GO



    update dbo.AppConfig set ConfigValue = 'false' where name = 'TurnOffStoreAdminEMailNotifications'


    update dbo.AppConfig SET ConfigValue = 'https://test.authorize.net/gateway/transact.dll'
    where name = 'AUTHORIZENET_TEST_SERVER'

    go

    -- Skipjack.DevServer renamed to Skipjack.TestServer
    if not exists(select * from dbo.AppConfig where name = 'Skipjack.TestServer')
     update dbo.AppConfig set Name = 'Skipjack.TestServer' where name = 'Skipjack.DevServer'
    go

    -- Skipjack.DevSerialNumber renamed to Skipjack.DeveloperSerialNumber
    if not exists(select * from dbo.AppConfig where name = 'Skipjack.DeveloperSerialNumber')
     update dbo.AppConfig set Name = 'Skipjack.DeveloperSerialNumber' where name = 'Skipjack.DevSerialNumber'
    go

    -- Skipjack.SerialNumber renamed to Skipjack.LiveSerialNumber
    if not exists(select * from dbo.AppConfig where name = 'Skipjack.LiveSerialNumber')
     update dbo.AppConfig set Name = 'Skipjack.LiveSerialNumber' where name = 'Skipjack.SerialNumber'
    go

	-- RTShipping Local Pickup appconfigs
	if not exists(select * from dbo.AppConfig where name = 'RTShipping.AllowLocalPickup')
	
	BEGIN
		INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'RTShipping.AllowLocalPickup','RTSHIPPING','Enabling this setting will allow a store-pickup option to be displayed and selected when using real time shipping rates.','false');
		INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'RTShipping.LocalPickupCost','RTSHIPPING','If you want to charge a handling fee for local in-store pickups, you can enter the amount here (eg. 10.00).','0.00');
		INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'RTShipping.LocalPickupRestrictionType','RTSHIPPING','If you want to limit the store-pickup option to only certain addresses or locations you can use this to determine how you are going to limit them. Unrestricted allows anyone to see and select the pickup option, state allows you to specify the states for which the pickup method is valid, zip allows you to specify a zip code or zip codes for which the pickup method is valid, and zone allows you to specify a zone or zones that are set up in the shipping zones section for which the pickup method is valid.','unrestricted');
		INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'RTShipping.LocalPickupRestrictionStates','RTSHIPPING','State restrictions for the store-pickup option if the restriction type is state.','');
		INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'RTShipping.LocalPickupRestrictionZips','RTSHIPPING','Zip Code restrictions for the store-pickup option if the restriction type is zip.','');
		INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'RTShipping.LocalPickupRestrictionZones','RTSHIPPING','Zone restrictions for the store-pickup option if the restriction type is zone.','');
	END
	
	go

	--RTSHIPPING LOCAL PICKUP
	UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.AllowLocalPickup'
	UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.LocalPickupCost'
	UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.LocalPickupRestrictionType'
	UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.LocalPickupRestrictionStates'
	UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.LocalPickupRestrictionZips'
	UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.LocalPickupRestrictionZones'





    --GIFT REGISTRY
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'AllowGiftRegistryQuantities'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'DecrementGiftRegistryOnOrder'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'ShowGiftRegistryButtons'

    --WishList
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'ShowWishButtons'  
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'DisallowAnonCustomerToCreateWishlist'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'AgeWishListDays'

    -- AFFILIATES
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'AffiliateEMailAddress'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'FilterDocumentsByAffiliate'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'FilterProductsByAffiliate'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'XmlPackage.AffiliateSignupNotification'



    -- AUSSIE POST/CANADA POST/DHL INTERNATIONAL
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.AusPost.DefaultPackageSize'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.AusPost.DomesticServices'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.AusPost.IntlServices'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.AusPost.MaxWeight'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'ShippingTrackingRegEx.AusPost'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'ShippingTrackingURL.AusPost' 
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.CanadaPost.DefaultPackageSize'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.CanadaPost.Language'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.CanadaPost.MaxWeight'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.CanadaPost.MerchantID'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.CanadaPost.Server'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.CanadaPost.ServerPort'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.DHLIntl.BillingAccountNbr'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.DHLIntl.BillingParty'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.DHLIntl.Dutiable'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.DHLIntl.DutyPayment'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.DHLIntl.DutyPaymentAccountNbr'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.DHLIntl.MaxWeight'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.DHLIntl.Overrides'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.DHLIntl.Packaging'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.DHLIntl.Services'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.DHLIntl.ShippingKey'

    -- GOOGLE CHECKOUT/GOOGLE ANALYTICS/GOOGLE SITEMAP
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.AllowAnonCheckout'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.AuthenticateCallback'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.BaseUrl'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.CarrierCalculatedDefaultPrice'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.CarrierCalculatedFreeOption'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.CarrierCalculatedPackage'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.CarrierCalculatedPackage'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.CarrierCalculatedShippingEnabled'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.CarrierCalculatedShippingOptions'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.ConversionParameters'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.ConversionURL'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultDomesticShipToCity'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultDomesticShipToCountry'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultDomesticShipToState'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultDomesticShipToZip'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultInternationalShipToCity'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultInternationalShipToCountry'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultInternationalShipToState'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultInternationalShipToZip'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultShippingMarkup'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultTaxRate'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DiagnosticsOnly'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.LiveCheckoutButton'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.LogFileName'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.LogMessages'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.CarrierCalculatedPackage'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.CarrierCalculatedShippingEnabled'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.CarrierCalculatedShippingOptions'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.ConversionParameters'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.ConversionURL'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultDomesticShipToCity'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultDomesticShipToCountry'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultDomesticShipToState'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultDomesticShipToZip'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultInternationalShipToCity'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultInternationalShipToCountry'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultInternationalShipToState'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultInternationalShipToZip'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultShippingMarkup'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DefaultTaxRate'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.DiagnosticsOnly'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.LiveCheckoutButton'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.LogFileName'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.LogMessages'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.MerchantId'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.MerchantKey'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.SandBoxCheckoutButton'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.SandBoxCheckoutURL'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.SandboxMerchantId'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.SandboxMerchantKey'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.SendStoreReceipt'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.ShippingIsTaxed'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.ShowOnCartPage'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.UseSandbox'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.UseTaxTables'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Google.AnalyticsAccount'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Google.EcomOrderTrackingEnabled'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'IncludeGoogleTrackingCode'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleSiteMap.EntityChangeFreq'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleSiteMap.EntityPriority'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleSiteMap.ObjectChangeFreq'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleSiteMap.ObjectPriority'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleSiteMap.TopicChangeFreq'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleSiteMap.TopicPriority'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleSiteMap.Xmlns'

    -- ANONYMOUS CHECKOUT
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'PasswordIsOptionalDuringCheckout'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'AnonCheckoutReqEmail'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'DisallowAnonCustomerToCreateWishlist'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'GoogleCheckout.AllowAnonCheckout'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'PayPal.Express.AllowAnonCheckout'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RatingsCanBeDoneByAnons'

    --ONE-PAGE-CHECKOUT 
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Checkout.UseOnePageCheckout'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Checkout.UseOnePageCheckout.UseFinalReviewOrderPage'

    --VAT/VAT MODE INC VAT & EX VAT
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'VAT.AllowCustomerToChooseSetting'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'VAT.CountryID'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'VAT.DefaultSetting'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'VAT.Enabled'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'VAT.HideTaxInOrderSummary'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'VAT.RoundPerItem'
        
    --BUYSAFE
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'BuySafe.Enabled'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'BuySafe.Hash'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'BuySafe.InvitationCode'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'BuySafe.JsUrl'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'BuySafe.MspId'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'BuySafe.Password'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'BuySafe.RegUrl'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'BuySafe.SealSize'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'BuySafe.StoreToken'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'BuySafe.Username'
        
    --CARDINAL
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'CardinalCommerce.Centinel.Enabled'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'CardinalCommerce.Centinel.IsLive'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'CardinalCommerce.Centinel.MapsTimeout'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'CardinalCommerce.Centinel.MerchantID'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'CardinalCommerce.Centinel.MsgType.Authenticate'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'CardinalCommerce.Centinel.MsgType.Lookup'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'CardinalCommerce.Centinel.MyECheckMarkAsCaptured'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'CardinalCommerce.Centinel.NumRetries'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'CardinalCommerce.Centinel.ProcessorID'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'CardinalCommerce.Centinel.TermURL'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'CardinalCommerce.Centinel.TransactionPwd'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'CardinalCommerce.Centinel.TransactionUrl.Live'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'CardinalCommerce.Centinel.TransactionUrl.Test'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'CardinalCommerce.Centinel.Version.Authenticate'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'CardinalCommerce.Centinel.Version.Lookup'
        
    --SHIPPING BY ZONE
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'ShippingCostWhenNoZoneMatch'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'ZoneIdForNoMatch'
        
    --PAYMENT GATEWAY
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'StoreCCInDB'
        
    --TEMPLATE SWITCHING
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'TemplateSwitching.Enabled'
        
    --ZOOMIFY
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Zoomify.ProductLarge'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Zoomify.Active'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Zoomify.GalleryMedium'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Zoomify.Large.Height'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Zoomify.Large.Width'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Zoomify.Medium.Height'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Zoomify.Medium.Width'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Zoomify.ProductMedium'
        
    --RECURRING BILLING / RECURRING PRODUCTS
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'AllowRecurringIntervalEditing'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'AUTHORIZENET_X_RECURRING_BILLING'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'eProcessingNetwork_X_RECURRING_BILLING'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'PayflowPro.RecurringMaxFailPayments'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'PayflowPro.Reporting.ReportName'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'QUICKCOMMERCE_X_RECURRING_BILLING'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Recurring.AUTHORIZENET_AUTOBILL_LIVE_SERVER'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Recurring.AUTHORIZENET_AUTOBILL_TEST_SERVER'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Recurring.ClearIsNewFlag'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Recurring.DefaultRecurringShippingMethod'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Recurring.DefaultRecurringShippingMethodID'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Recurring.GatewayImportOffsetHours '
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Recurring.GatewayLastImportedDate '
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Recurring.LimitCustomerToOneOrder'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Recurring.SendOrderEMailToCustomer'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Recurring.SendShippedEMail'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Recurring.UseGatewayInternalBilling'
        
    --MULTI-SHIP
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'MultiShipMaxItemsAllowed'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'AllowMultipleShippingAddressPerOrder'
    -- DROP-SHIP (CHECKOUT)	
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'AllowAddressChangeOnCheckoutShipping'
        
    --CUSTOMER LEVEL
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'CustomerLevel0AllowsPOs'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'DefaultCustomerLevelID'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'FilterByCustomerLevelIsAscending'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'FilterDocumentsByCustomerLevel'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'FilterProductsByCustomerLevel'
        
    --POLL & REVIEW
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Polls.Enabled'
       
    --SUBSCRIPTION / SUBSCRIPTION PRODUCTS / TOPICS
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'SubscriptionExpiredGracePeriod'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'SubscriptionExtensionOccursFromOrderDate'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Suscription.ExpiredMessageWhenViewingTopic'
        
    --DISTRIBUTOR
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Default_DistributorColWidth'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Default_DistributorPageSize'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'DistributorEMailCC'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'DistributorImg_icon'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'DistributorImg_large'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'DistributorImg_medium' 
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.MultiDistributorCalculation'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Search_ShowDistributorsInResults'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'SearchAdv_ShowDistributor'
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'XmlPackage.DefaultDistributorNotification'


    --Fedex Shipping Manager\
        UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'FedexShipManager.Enabled' 
        
        
    --DISTRIBUTOR
    UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Default_DistributorColWidth' 
    UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Default_DistributorPageSize'
    UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'DistributorEMailCC'
    UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'DistributorImg_icon'
    UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'DistributorImg_large'
    UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'DistributorImg_medium'
    UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'RTShipping.MultiDistributorCalculation'
    UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'Search_ShowDistributorsInResults'
    UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'SearchAdv_ShowDistributor'
    UPDATE dbo.Appconfig set Hidden = 1 where [name] = 'XmlPackage.DefaultDistributorNotification'


    --PAYMENT GATEWAYS
    UPDATE dbo.Appconfig set Hidden = 1 where Name 
        NOT like '%AUTHORIZENET%'
        AND Name NOT like '%CYBERSOURCE%'
        AND Name NOT like '%ESELECTPLUS%'
        AND Name NOT like '%PAYMENTECH%'
        AND Name NOT like '%PAYFLOWPRO%'
        AND Name NOT like '%PAYPAL%'
        --this is not a gateway specific settings so dont hide it
        AND Name <> 'CardExtraCodeIsOptional'
        AND Name <> 'MailCheckReminder'
        AND Name <> 'MinCartItemsBeforeCheckout'
        AND Name <> 'PaymentGateway'
        AND Name <> 'PaymentMethods'
        AND Name <> 'UseLiveTransactions'
        AND Name <> '3DSecure.CreditCardTypeIDs'
        AND Name <> 'MaxMind.Enabled'
        AND Name <> 'MaxMind.LicenseKey'
        AND Name <> 'MaxMind.FailScoreThreshold'
        AND Name <> 'MaxMind.DelayDownloadThreshold'
        AND Name <> 'MaxMind.DelayDropShipThreshold'
        AND Name <> 'MaxMind.SOAPURL'
        AND Name <> 'GatewayRetries'
        ----------			
        AND GroupName = 'GATEWAY'
        OR GroupName = 'GATEWAYS'




    GO

























    delete dbo.AppConfig where name = 'RTShipping.USPS.Password'

    delete dbo.AppConfig where name = 'Buysafe.WebReferenceURL'

    -- this appconfig is now obsolete:
    delete from [dbo].AppConfig where name='EncryptPassword';

    -- obsolete PayPal Pro AppConfigs:
    delete dbo.AppConfig where [name] ='PayPalPro.LiveCertFile'
    delete dbo.AppConfig where [name] ='PayPalPro.SandboxCertFile'
    delete dbo.AppConfig where [name] ='PayPalPro.ObjRequestVersion'
    delete dbo.AppConfig where [name] ='DropShipEnabled' -- obsolete AppConfig

    delete from [dbo].AppConfig where name='FilterByCustomerLevel0SeesUnmappedProducts';
    delete from [dbo].AppConfig where name='FilterByCustomerLevel0SeesUnmappedDocuments';
    delete from [dbo].AppConfig where name='Dispatch_CellCarrier';
    delete from [dbo].AppConfig where name='Admin_LogSQL';
    delete from [dbo].AppConfig where name='ImageFrameStyle';
    delete from [dbo].AppConfig where name='BestSellersShowSales';
    delete from [dbo].AppConfig where name='BestSellersShowPics';
    delete from [dbo].AppConfig where name='BestSellersShowPicsHeight';
    delete from [dbo].AppConfig where name='CategoryGridPageSize';
    delete from [dbo].AppConfig where name='ForceCategoryHeaderDisplay';
    delete from [dbo].AppConfig where name='ForceManufacturerHeaderDisplay';
    delete from [dbo].AppConfig where name='ForceSectionHeaderDisplay';
    delete from [dbo].AppConfig where name='Froogle_FTPServer';
    delete from [dbo].AppConfig where name='Froogle_FTPPort';
    delete from [dbo].AppConfig where name='Froogle_UserName';
    delete from [dbo].AppConfig where name='Froogle_Password';
    delete from [dbo].AppConfig where name='Froogle_FileName';
    delete from [dbo].AppConfig where name='Froogle_UseDescriptionsIfFroogleDescEmpty';
    delete from [dbo].AppConfig where name='IonGateLogin';
    delete from [dbo].AppConfig where name='IonGateLoginTest';
    delete from [dbo].AppConfig where name='PageNumberFormat';
    delete from [dbo].AppConfig where name='RecentAdditionsN';
    delete from [dbo].AppConfig where name='RecentAdditionsNumDays';
    delete from [dbo].AppConfig where name='RecentAdditionsShowPics';
    delete from [dbo].AppConfig where name='RecentAdditionsShowPicsHeight';
    delete from [dbo].AppConfig where name='RTShipping.Carrier';
    delete from [dbo].AppConfig where name='SectionGridPageSize';
    delete from [dbo].AppConfig where name='ShowEmptyCategories';
    delete from [dbo].AppConfig where name='ShowEmptyManufacturers';
    delete from [dbo].AppConfig where name='ShowEmptySections';
    delete from [dbo].AppConfig where name='ShowFullNameInGrid';
    delete from [dbo].AppConfig where name='ShowLeftCol';
    delete from [dbo].AppConfig where name='ShowPriceInCategoryGrid';
    delete from [dbo].AppConfig where name='ShowPriceInSectionGrid';
    delete from [dbo].AppConfig where name='ShowPriceSalePrompt';
    delete from [dbo].AppConfig where name='ShowPriceExtendedPrompt';
    delete from [dbo].AppConfig where name='ShowRightCol';
    delete from [dbo].AppConfig where name='ShowSectionsForProduct';
    delete from [dbo].AppConfig where name='ShowSubSectionsInGrid';
    delete from [dbo].AppConfig where name='ShowSortByPrice';
    delete from [dbo].AppConfig where name='CondensedTablePictureHeight';
    delete from [dbo].AppConfig where name='ShowTreeInAdmin';
    delete from [dbo].AppConfig where name='CategoryGridCellVerticalAlignment';
    delete from [dbo].AppConfig where name='OpenCubeEnabled';
    delete from [dbo].AppConfig where name='MaxNumRowsToDisableAllLink';
    delete from [dbo].AppConfig where name='ManufacturerGridPageSize';
    delete from [dbo].AppConfig where name='LibraryGridPageSize';
    delete from [dbo].AppConfig where name='QuickBooks.Version';
    delete from [dbo].AppConfig where name='Admin_DefaultCategoryColWidth';
    delete from [dbo].AppConfig where name='Admin_DefaultManufacturerColWidth';
    delete from [dbo].AppConfig where name='Admin_DefaultProductColWidth';
    delete from [dbo].AppConfig where name='Admin_DefaultSectionColWidth';
    delete from [dbo].AppConfig where name='DefaultCategoryColWidth';
    delete from [dbo].AppConfig where name='DefaultManufacturerColWidth';
    delete from [dbo].AppConfig where name='DefaultProductColWidth';
    delete from [dbo].AppConfig where name='DefaultSectionColWidth';
    delete from [dbo].AppConfig where name='Xml.DefaultPage';
    delete from [dbo].AppConfig where name='Xml.ShoppingCartPageHeader';
    delete from [dbo].AppConfig where name='Xml.ShoppingCartPageFooter';
    delete from [dbo].AppConfig where name='Xml.OrderConfirmationPage';
    delete from [dbo].AppConfig where name='Xml.WishListPage';
    delete from [dbo].AppConfig where name='Xml.GiftRegistryPage';
    delete from [dbo].AppConfig where name='Xml.UpsellProductPackage';
    delete from [dbo].AppConfig where name='Admin_DefaultCategoryDisplayFormatID';
    delete from [dbo].AppConfig where name='Admin_DefaultManufacturerDisplayFormatID';
    delete from [dbo].AppConfig where name='Admin_DefaultDistributorDisplayFormatID';
    delete from [dbo].AppConfig where name='Admin_DefaultProductDisplayFormatID';
    delete from [dbo].AppConfig where name='Admin_DefaultSectionDisplayFormatID';



    delete dbo.AppConfig where name = 'MaxMind.UseHTTPSCall'
    delete dbo.AppConfig where name = 'MaxMind.Timeout'

    delete dbo.AppConfig where name = 'PayPal.API.Version'
    delete dbo.AppConfig where name = 'PayPal.API.RefundVersion'
    delete dbo.AppConfig where name = 'PayPal.API.LiveAAURL'
    delete dbo.AppConfig where name = 'PayPal.API.TestAAURL'
    delete dbo.AppConfig where name = 'PayPalPro.AppId'
    delete dbo.AppConfig where name = 'PayPalPro.DevId'

    delete dbo.AppConfig where name = 'SearchAdv_ShowPicsHeight'

    delete dbo.AppConfig where name = 'XmlPackage.IncludeCookies'

    delete dbo.AppConfig where name = 'PricesEnteredWithVAT'

    delete dbo.Appconfig WHERE [Name] = 'GoogleCheckout.HideCouponOnCartPage'

    go













    declare @ConfigValue nvarchar(1000)
    SELECT @ConfigValue = ConfigValue FROM dbo.AppConfig WHERE [Name] = 'Admin_SuperUser'
    update Customer set IsAdmin = 3
    from Customer c join dbo.split(@ConfigValue, ',') s on c.customerid = convert(int, s.items)
    go

    declare @defaultname nvarchar(4000)
    select @defaultname = name From sysobjects where id = (select cdefault From syscolumns where id = object_id('Customer') and name = 'EMail')
    if @defaultname is not null begin
        declare @cmd nvarchar(4000)
        select @cmd = 'alter table dbo.Customer drop constraint ' + @defaultname
        EXEC (@cmd)
    end

    alter table dbo.customer add constraint DF_Customer_EMail DEFAULT '' for EMail
    go

    delete from dbo.Customer where EMail like 'Anon[_]%'
                and customerid not in (select customerid from dbo.ShoppingCart with (NOLOCK)) 
                and customerid not in (select customerid from dbo.kitcart with (NOLOCK)) 
                and customerid not in (select customerid from dbo.customcart with (NOLOCK)) 
                and customerid not in (select customerid from dbo.orders with (NOLOCK)) 
                and customerid not in (select customerid from dbo.rating with (NOLOCK)) 
                and customerid not in (select ratingcustomerid from dbo.ratingcommenthelpfulness with (NOLOCK)) 
                and customerid not in (select votingcustomerid from dbo.ratingcommenthelpfulness with (NOLOCK)) 
                and customerid not in (select customerid from dbo.pollvotingrecord with (NOLOCK));
    go

    update dbo.Customer Set EMail='' where EMail like 'Anon[_]%';
    go

    update dbo.Orders Set EMail='' where EMail like 'Anon[_]%';
    go

    update dbo.Affiliate Set EMail='' where EMail like 'Anon[_]%';
    go

    update dbo.Address Set EMail='' where EMail like 'Anon[_]%';
    go


    exec aspdnsf_CustomerConsistencyCheck;
    go

    -- time to get rid of this VERY VERY old construct (4 years old)
    update dbo.Customer Set Password='' where Password='N/A';
    go

    -- we have to create the IsRegistered flag for the customer, we can only approximate this by the following statement, as older versions did not have other data:
    update dbo.Customer set IsRegistered = 1 where IsAdmin<>0 or (EMail <> '' and isnull(Password, '') <> '');
    go

    -- NO LONGER USED!! per PABP mods
    delete dbo.AppConfig where [name] = 'Admin_Superuser'
    go






    /*  SET HISTORY FLAG FOR OLD ORDERS TO AVOID DOWNLOADING HISTORICAL DATA */
    update [dbo].Orders SET THUB_POSTED_TO_ACCOUNTING = 'H';
    GO



    if not exists(select * from dbo.Topic where name = 'buySAFEOrderOptionDescription')
        INSERT [dbo].Topic(Name,HTMLOk,ShowInSiteMap,Title,Description) values('buySAFEOrderOptionDescription',1,0,'BuySafe','buySAFE is the e-commerce Bonding company that ensures internet shopping is always reliable, trusted and risk-free.  Professional online merchants partner with buySAFE to provide their buyers ultimate peace of mind.  They do this by having buySAFE:<br/><br/><ul><li>inspect their website operations,</li><li>monitor their performance daily and</li><li>bond your purchases up to $25,000</li></ul><br/><br/>The display of the buySAFE Seal on a merchant’s site means you can trust they are one of the best merchants on the web.<br/><br/>buySAFE’s bonding partners are Liberty Mutual, Travelers & ACE USA.')
    go

    if not exists(select * from dbo.Topic where name = 'EMailError')
        INSERT [dbo].topic(Name,HTMLOk,ShowInSiteMap,Title,Description) values('EMailError'					,1,0,'EMailError'																	,'This page could not be shown, if you are using a personal firewall, it may be interfering with the internal security checks of AspDotNetStorefront, to prevent attacks. This should not occur on the production site, when properly configured and setup.');
    go

    if not exists(select * from dbo.Topic where name = 'GoogleCheckout')
        INSERT [dbo].topic(Name,HTMLOk,ShowInSiteMap,Title,Description) values('GoogleCheckout',1,0,'GoogleCheckout','<p>In an effort to provide our users the broadest selection of payment options,&nbsp; this store has implemented Google''s new payment service ''Google Checkout''. The Google Checkout service assists customers in two distinct ways.<br><br>On the search side, it shows Google Checkout customers more relevant information in certain Google search results by displaying Google Checkout badging in connection with those merchants who are currently using the Google Checkout process <br><br>Once at the merchant’s web site, Google Checkout (i) provides you with a payment mechanism whereby you don’t need to share your credit card information with the merchant, (ii) moves you through checkout with a single login for use across multiple stores, and (iii) enables you to track your purchase histories in one place at Google.<br><br>Important Google Checkout features are as follows: &nbsp;&nbsp;&nbsp;&nbsp;</p><ul>	<li>Credit/Debit Card Information - Your 	credit/debit card information is maintained by Google. Charges to your 	credit card are made by Google Payment Corporation, and your credit/debit 	card information is not shared with&nbsp; this store or any other merchant that 	uses Google Checkout &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</li>	<li>Maintained Communications - Once you have 	completed a purchase using Google Checkout,&nbsp; this store will maintain 	communications with you regarding shipping, changes to the order, tracking 	and returns. These communications will be made to the email address listed 	in your Google Checkout account and are vital in providing you with 	important information regarding the fulfillment of your order. &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</li>	<li>Universal Purchase History - You will have 	a universal purchase history for all purchases made using Google Checkout 	whether from&nbsp; this store or other merchants using Google Checkout. This 	universal purchase history will be maintained by Google in your Google 	Checkout account.&nbsp;<br>&nbsp;</li></ul>');
    go

    if not exists(select * from dbo.Topic where name = 'EmptyGenreText')
        INSERT [dbo].Topic(Name,HTMLOk,ShowInSiteMap,Title,Description) values('EmptyGenreText',1,0,'EmtpyGenreText','<b>We currently do not have any products for this Genre. Please check back soon for new products... This is the topic Named: emptyGenretext. You edit this in the ADMIN site.</b>');
    go

    if not exists(select * from dbo.Topic where name = 'EmptyVectorText')
        INSERT [dbo].Topic(Name,HTMLOk,ShowInSiteMap,Title,Description) values('EmptyVectorText',1,0,'EmtpyVectorText','<b>We currently do not have any products for this Vector. Please check back soon for new products... This is the topic Named: emptyVectortext. You edit this in the ADMIN site.</b>');
    go

    if not exists(select * from dbo.Topic where name = '3DSecureExplanation')
        INSERT [dbo].Topic(Name,HTMLOk,ShowInSiteMap,Title,Description) values('3DSecureExplanation',1,0,'3DSecureExplanation','<b>Verified By Visa<br/>MasterCard Secure<br/>Validation Step</b><br/><br/>You will now be shown a form from your credit card issuing bank. This form is entirely private between you and your bank, and the form is secure. The information you enter here is <b>not</b> shared with us. This is to provide additional buyer protection on your credit card. Do not close this window or hit the Back button.</b><br/><br/>Payment authentication is in progress. Please wait until the authentication screen has loaded, then enter your information and click submit.<br/><br/>If you wish to cancel your checkout, please <a href="shoppingcart.aspx">click here</a>.');
    go

    if not exists(select * from dbo.Topic where name = 'PhoneOrderTimeout')
        INSERT [dbo].topic(Name,HTMLOk,ShowInSiteMap,Title,Description) values('PhoneOrderTimeout',1,0,'PhoneOrderTimeout','Your phone order session has timed out. Please restart your phone order session by using the buttons above...');
    go

    if not exists(select * from dbo.Topic where name = 'BuySafe')
        INSERT [dbo].Topic(Name,HTMLOk,ShowInSiteMap,Title,Description) values('BuySafe', 1,0,'BuySafe','<style type=text/css>   <!--    /* Font Definitions */   @font-face     {font-family:Verdana;     panose-1:2 11 6 4 3 5 4 4 2 4;}        /* Style Definitions */    p.MsoNormal, li.MsoNormal, div.MsoNormal     {margin:0in;     margin-bottom:.0001pt;     font-size:10.0pt;     font-family:"Verdana";}   p     {margin-top:1.5pt;     margin-right:0in;     margin-bottom:6.0pt;     margin-left:0in;     font-size:10.0pt;     font-family:"Verdana";}   @page Section1     {size:8.5in 11.0in;     margin:1.0in 1.25in 1.0in 1.25in;}   div.Section1     {page:Section1;}    /* List Definitions */   ol     {margin-bottom:0in;}   ul     {margin-bottom:0in;}   -->   </style>   <div class=Section1> <table width="100%" border=0> <tbody> <tr> <td style="WIDTH: 20%">&nbsp;</td> <td style="WIDTH: 60%"> <p><span style="FONT-SIZE: 18pt"> <center><strong>BuySafe registration:</strong></center></span></td> <td style="WIDTH: 20%"><img alt="" src="images/buysafe_logo.jpg"></td></tr></tbody></table><br> <p></p> <p><b>Introducing buySAFE – the strongest trust signal in e-Commerce!</b> </p> <table> <tbody> <tr> <td>&nbsp; <p><b>Turn More Shoppers Into Buyers With buySAFE</b> </p> <p>Not all shoppers feel safe and confident when buying online and this costs merchants like you money every day. To effectively compete, you need all your shoppers to feel 100% confident when visiting your website. You need buySAFE! </p> <p><b><i>buySAFE provides the only explicit, third-party endorsement of a merchant’s trustworthiness and reliability – backed by a bond guarantee.</i></b> </p> <p><b><u>With buySAFE, online merchants can expect:</u></b> </p> <ul style="MARGIN-TOP: 0in" type=disc> <li><b>Increased website conversion rates</b> – 6.7% average increase <li><b>More repeat buyers</b> – 4.3% more likely to buy again and again <li><b>Greater profits</b> – more carts started and fewer carts abandoned add up to more profits for you! </li></ul></td> <td> <table border=1> <tbody> <tr> <td> <p><b>Introductory Offer for AspDotNetStorefront Merchants: Try buySAFE and get TWO months of hosting FREE!</b></p> <p>For a limited time, install buySAFE in your AspDotNet store, and buySAFE will cover your hosting bill for the next TWO months - up to a $250!</p> <p style="TEXT-ALIGN: center" align=center>Visit <a href="http://www.buysafe.com/offerASPDOTNET">www.buySAFE.com/offerASPDOTNET</a> for details.</p></td></tr></tbody></table></td></tr></tbody></table> <p>Best of all, buySAFE is FREE for AspDotNetStorefront merchants – there are no hidden fees, no risk, and no long-term commitments! </p> <p style="TEXT-ALIGN: center" align=center>Learn more at <a href="http://www.buysafe.com/offerASPDOTNET" target=new>www.buySAFE.com/offerASPDOTNET</a> </p> <p><b>How does buySAFE work?</b> </p> <p>Approved buySAFE merchants display the buySAFE Seal on their websites—the only seal that explicitly certifies that an online merchant is reliable and trustworthy. The buySAFE bond guarantee at checkout, provides shoppers with the option to fully guarantee their online purchase with a buySAFE bond. Together, this powerful combination makes shoppers feel safer and more confident and, therefore, more likely to buy from you. </p> <p><b>Get Started Now!</b> </p> <p><b>Here are the simple steps to enable buySAFE in your online store:</b> </p> <ol style="MARGIN-TOP: 0in" type=1> <li>Click on “Register” below and complete the short buySAFE Merchant application and store setup process. <li>Next, from your AspDotNetStorefront admin dashboard, choose “buySAFE Authentication” under the “buySAFE” menu. You’ll see the Store and Seal Authentication populated with your data. <li>Click on “Test Authentication” which verifies your data. <li>Lastly, go to your AspDotNet online store and make sure the buySAFE Seal and bond guaranteed are visible and working. </li></ol> <p>&nbsp; </p> <p>Once you complete these steps, you’ll be set up to start turning more shoppers into buyers with buySAFE! </p> <p>&nbsp; </p> <p style="TEXT-ALIGN: center" align=center><b><span style="COLOR: #b20000">Click “Register” below to get started!</span></b> </p></div> ')
    go

    if not exists(select * from dbo.Topic where name = 'CardinalMyECheckPageHeader')
        INSERT [dbo].Topic(Name,HTMLOk,ShowInSiteMap,Title,Description) values('CardinalMyECheckPageHeader'	,1,0,'eCheck Terms','your MyECheck terms here. This is the topic Named: CardinalMyECheckPageHeader. You edit this in the ADMIN site.');
    go
    if not exists(select * from dbo.Topic where name = 'CardinalMyECheckExplanation')
        INSERT [dbo].Topic(Name,HTMLOk,ShowInSiteMap,Title,Description) values('CardinalMyECheckExplanation' ,1,0,'eCheck Processing','You will now be shown a form from our eCheck processing service. The information you enter here is <b>not</b> shared with us. Do not close this window or hit the Back button.<br/><br/>Payment authentication is in progress. Please wait until the MyECheck screen has loaded, then enter your information and click submit.<br/><br/>If you wish to cancel your checkout, please <a href="shoppingcart.aspx">click here</a>.');
    go


    -- PageNotFound
    if not exists(select * from dbo.Topic where name = 'PageNotFound')
        INSERT [dbo].Topic([Name],Title,Description,ShowInSiteMap) values('PageNotFound','PageNotFound','<font color=#ff0000 size=2>  <p><br><font size=4>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp;Ooops, sorry the page you requested is not existing</font></p></font>',0)
    go



    update dbo.Topic
    set Description = 'Register Now For Additional Site Features:<br/><ul><li>Remember your cart for next time</li><li>Remember your order history</li><li>Create your shipping/billing address book</li><li>Setup a wish list</li><li>Easily re-order again</li></ul>'
    where Name = 'CheckoutAnonTeaser'
    go


    -- repeated here, in case prior upgrade wasn't done:
    update [dbo].topic set HTMLOk=0 where Name in ('GoogleTrackingCode','OvertureTrackingCode');
    go

    UPDATE [dbo].Topic
    SET Description = 'This is the topic Named: contact. You edit this in the ADMIN site. Your Store Name<br/>Address1<br/>Address2<br/>City, State Zip<br/>Phone: xxx-xxx-xxx<br/>FAX: xxx-xxx-xxx<br/>E-Mail: sales@yourdomain.com<br/><br/><hr size="1"/><b>Or, use the form below to send us a message</b><br/><br/><form id="MailArea" method="POST" action="sendform.aspx"><div><script type="text/javascript">function CheckFields(){var ergx = new RegExp(''\\w+([-+.\'']\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*''); if(document.getElementById("MailName").value == "" || document.getElementById("MailPhone").value == "" || document.getElementById("MailEmail").value == "" || document.getElementById("MailSubject").value == "" || document.getElementById("MailMessage").value == ""){alert("Please complete entries.");return false;} if(!ergx.test(document.getElementById("MailEmail").value)){alert("Invalid Email.");return false;}}</script></div><div align="left"><table border="0" cellpadding="0" cellspacing="0" width="454"><tr><td valign="top" align="right" width="102">*Your Name:</td><td valign="middle" align="left" width="348"><input type="text" Name="Name" size="27" id="MailName"></td></tr><tr><td valign="top" align="right" width="102">*Your Phone:</td><td valign="middle" align="left" width="348"><input type="text" Name="Phone" size="27" id="MailPhone"></td></tr><tr><td valign="top" align="right" width="102">*Your E-Mail:</td><td valign="middle" align="left" width="348"><input type="text" Name="EMail" size="27" id="MailEmail"></td></tr><tr><td valign="top" align="right" width="102">*Subject:</td><td valign="middle" align="left" width="348"><input type="text" Name="Subject" size="27" id="MailSubject"></td></tr><tr><td valign="top" align="right" width="102">*Message:</td><td valign="middle" align="left" width="348"><textarea rows="11" Name="S1" cols="41" id="MailMessage"></textarea></td></tr><tr><td width="102"></td><td width="348"><input type="Submit" value="Submit" Name="B1" onclick="return CheckFields();"></td></tr></table></div></form>'
    WHERE Name = 'contact'

    go










    update [dbo].Orders set IsPrinted=1, ReadyToShip=1 where ShippedOn IS NOT NULL


    set IDENTITY_insert [dbo].ProductType ON;
    insert [dbo].ProductType(ProductTypeID,Name) values(100,'Gift Card Physical');
    insert [dbo].ProductType(ProductTypeID,Name) values(101,'Gift Card Email');
    insert [dbo].ProductType(ProductTypeID,Name) values(102,'Gift Card Certificate');
    set IDENTITY_insert [dbo].ProductType OFF;
    update [dbo].Product set ProductTypeID=100 where Name='Gift Card';
    GO


    set IDENTITY_insert [dbo].Currency ON;
    insert [dbo].Currency(CurrencyID,Name,CurrencyCode,ExchangeRate,Published,DisplayOrder,DisplayLocaleFormat) values(1,'US Dollar','USD',1.00,1,1,'en-US');
    set IDENTITY_insert [dbo].Currency OFF;
    GO



    -- convert all displayformatid's to xmlpackages now:
    update [dbo].Category set XmlPackage='entity.grid.xml.config' where CategoryDisplayFormatID=1;
    go

    update [dbo].Category set XmlPackage='entity.grid.xml.config' where CategoryDisplayFormatID=1;
    go
    update [dbo].Category set XmlPackage='entity.tableexpanded.xml.config' where CategoryDisplayFormatID=2;
    go
    update [dbo].Category set XmlPackage='entity.tablecondensed.xml.config' where CategoryDisplayFormatID=3;
    go
    update [dbo].Category set XmlPackage='entity.simpleproductlist.xml.config' where CategoryDisplayFormatID=4;
    go
    update [dbo].Category set XmlPackage='entity.tableorderform.xml.config' where CategoryDisplayFormatID=5;
    go
    update [dbo].Category set XmlPackage='entity.onelineproductlist.xml.config' where CategoryDisplayFormatID=6;
    go
    update [dbo].Category set XmlPackage='entity.tableexpanded2.xml.config' where CategoryDisplayFormatID=7;
    go
    update [dbo].Category set XmlPackage='entity.tablecondensed2.xml.config' where CategoryDisplayFormatID=8;
    go

    update [dbo].Section set XmlPackage='entity.grid.xml.config' where SectionDisplayFormatID=1;
    go
    update [dbo].Section set XmlPackage='entity.tableexpanded.xml.config' where SectionDisplayFormatID=2;
    go
    update [dbo].Section set XmlPackage='entity.tablecondensed.xml.config' where SectionDisplayFormatID=3;
    go
    update [dbo].Section set XmlPackage='entity.simpleproductlist.xml.config' where SectionDisplayFormatID=4;
    go
    update [dbo].Section set XmlPackage='entity.tableorderform.xml.config' where SectionDisplayFormatID=5;
    go
    update [dbo].Section set XmlPackage='entity.onelineproductlist.xml.config' where SectionDisplayFormatID=6;
    go
    update [dbo].Section set XmlPackage='entity.tableexpanded2.xml.config' where SectionDisplayFormatID=7;
    go
    update [dbo].Section set XmlPackage='entity.tablecondensed2.xml.config' where SectionDisplayFormatID=8;
    go

    update [dbo].Library set XmlPackage='entity.grid.xml.config' where LibraryDisplayFormatID=1;
    go
    update [dbo].Library set XmlPackage='entity.tableexpanded.xml.config' where LibraryDisplayFormatID=2;
    go
    update [dbo].Library set XmlPackage='entity.tablecondensed.xml.config' where LibraryDisplayFormatID=3;
    go
    update [dbo].Library set XmlPackage='entity.simpleproductlist.xml.config' where LibraryDisplayFormatID=4;
    go
    update [dbo].Library set XmlPackage='entity.tableorderform.xml.config' where LibraryDisplayFormatID=5;
    go
    update [dbo].Library set XmlPackage='entity.onelineproductlist.xml.config' where LibraryDisplayFormatID=6;
    go
    update [dbo].Library set XmlPackage='entity.tableexpanded2.xml.config' where LibraryDisplayFormatID=7;
    go
    update [dbo].Library set XmlPackage='entity.tablecondensed2.xml.config' where LibraryDisplayFormatID=8;
    go

    update [dbo].Manufacturer set XmlPackage='entity.grid.xml.config' where ManufacturerDisplayFormatID=1;
    go
    update [dbo].Manufacturer set XmlPackage='entity.tableexpanded.xml.config' where ManufacturerDisplayFormatID=2;
    go
    update [dbo].Manufacturer set XmlPackage='entity.tablecondensed.xml.config' where ManufacturerDisplayFormatID=3;
    go

    update [dbo].Distributor set XmlPackage='entity.grid.xml.config' where DistributorDisplayFormatID=1;
    go
    update [dbo].Distributor set XmlPackage='entity.tableexpanded.xml.config' where DistributorDisplayFormatID=2;
    go
    update [dbo].Distributor set XmlPackage='entity.tablecondensed.xml.config' where DistributorDisplayFormatID=3;
    go

    update [dbo].product set XmlPackage='product.kitproduct.xml.config' where IsAKit=1 and IsAPack=0 and XmlPackage IS NULL or XmlPackage='';
    go
    update [dbo].product set XmlPackage='product.packproduct.xml.config' where IsAKit=0 and IsAPack=1 and XmlPackage IS NULL or XmlPackage='';
    go
    update [dbo].Product set XmlPackage='product.VariantsInRightBar.xml.config' where XmlPackage IS NULL and ProductDisplayFormatID=1;
    go
    update [dbo].Product set XmlPackage='product.VariantsInGrid.xml.config' where XmlPackage IS NULL and ProductDisplayFormatID=2;
    go
    update [dbo].Product set XmlPackage='product.VariantsInTableExpanded.xml.config' where XmlPackage IS NULL and ProductDisplayFormatID=3;
    go
    update [dbo].Product set XmlPackage='product.VariantsInTableCondensed.xml.config' where XmlPackage IS NULL and ProductDisplayFormatID=4;
    go
    update [dbo].Product set XmlPackage='product.SimpleProduct.xml.config' where XmlPackage IS NULL and ProductDisplayFormatID=5;
    go
    update [dbo].Product set XmlPackage='product.SimpleProduct.xml.config' where XmlPackage IS NULL and ProductDisplayFormatID=6;
    go




    declare @def varchar(255)
    select @def = so.name From dbo.sysobjects so join [dbo].syscolumns sc on so.id = sc.cdefault and sc.id = object_id('CATEGORY') and sc.name = 'CATEGORYDisplayFormatID'
    if @@rowcount > 0 begin
        exec ('ALTER TABLE CATEGORY DROP CONSTRAINT ' + @def)
        ALTER TABLE dbo.CATEGORY DROP COLUMN CATEGORYDisplayFormatID
    end
    go

    declare @def varchar(255)
    select @def = so.name From dbo.sysobjects so join [dbo].syscolumns sc on so.id = sc.cdefault and sc.id = object_id('SECTION') and sc.name = 'SECTIONDisplayFormatID'
    if @@rowcount > 0 begin
        exec ('ALTER TABLE SECTION DROP CONSTRAINT ' + @def)
        ALTER TABLE dbo.SECTION DROP COLUMN SECTIONDisplayFormatID
    end
    go

    declare @def varchar(255)
    select @def = so.name From dbo.sysobjects so join [dbo].syscolumns sc on so.id = sc.cdefault and sc.id = object_id('LIBRARY') and sc.name = 'LIBRARYDisplayFormatID'
    if @@rowcount > 0 begin
        exec ('ALTER TABLE LIBRARY DROP CONSTRAINT ' + @def)
        ALTER TABLE dbo.LIBRARY DROP COLUMN LIBRARYDisplayFormatID
    end
    go

    declare @def varchar(255)
    select @def = so.name From dbo.sysobjects so join [dbo].syscolumns sc on so.id = sc.cdefault and sc.id = object_id('MANUFACTURER') and sc.name = 'MANUFACTURERDisplayFormatID'
    if @@rowcount > 0 begin
        exec ('ALTER TABLE MANUFACTURER DROP CONSTRAINT ' + @def)
        ALTER TABLE dbo.MANUFACTURER DROP COLUMN MANUFACTURERDisplayFormatID
    end
    go

    declare @def varchar(255)
    select @def = so.name From dbo.sysobjects so join [dbo].syscolumns sc on so.id = sc.cdefault and sc.id = object_id('DISTRIBUTOR') and sc.name = 'DISTRIBUTORDisplayFormatID'
    if @@rowcount > 0 begin
        exec ('ALTER TABLE DISTRIBUTOR DROP CONSTRAINT ' + @def)
        ALTER TABLE dbo.DISTRIBUTOR DROP COLUMN DISTRIBUTORDisplayFormatID
    end
    go

    declare @def varchar(255)
    select @def = so.name From dbo.sysobjects so join [dbo].syscolumns sc on so.id = sc.cdefault and sc.id = object_id('PRODUCT') and sc.name = 'PRODUCTDisplayFormatID'
    if @@rowcount > 0 begin
        exec ('ALTER TABLE PRODUCT DROP CONSTRAINT ' + @def)
        ALTER TABLE dbo.PRODUCT DROP COLUMN PRODUCTDisplayFormatID
    end
    go

    -- this one may fail, that is ok:
    declare @def varchar(255)
    select @def = so.name From dbo.sysobjects so join [dbo].syscolumns sc on so.id = sc.cdefault and sc.id = object_id('DOCUMENT') and sc.name = 'DOCUMENTDisplayFormatID'
    if @@rowcount > 0 begin
        exec ('ALTER TABLE DOCUMENT DROP CONSTRAINT ' + @def)
        ALTER TABLE dbo.DOCUMENT DROP COLUMN DOCUMENTDisplayFormatID
    end
    go


    if exists (select * From sysobjects where id = object_id('CategoryDisplayFormat') and type = 'u')
        drop table dbo.CategoryDisplayFormat;
    go
    if exists (select * From sysobjects where id = object_id('SectionDisplayFormat') and type = 'u')
        drop table dbo.SectionDisplayFormat;
    go
    if exists (select * From sysobjects where id = object_id('LibraryDisplayFormat') and type = 'u')
        drop table dbo.LibraryDisplayFormat;
    go
    if exists (select * From sysobjects where id = object_id('ManufacturerDisplayFormat') and type = 'u')
        drop table dbo.ManufacturerDisplayFormat;
    go
    if exists (select * From sysobjects where id = object_id('DistributorDisplayFormat') and type = 'u')
        drop table dbo.DistributorDisplayFormat;
    go
    if exists (select * From sysobjects where id = object_id('ProductDisplayFormat') and type = 'u')
        drop table dbo.ProductDisplayFormat;
    go

    -- this one may fail, that is ok:
    if exists (select * From sysobjects where name = 'DocumentDisplayFormat' and type = 'u')
        drop table dbo.DocumentDisplayFormat;
    go



    if exists (select * From sysobjects where id = object_id('CategoryDisplayOrder') and type = 'u')
        drop table dbo.CategoryDisplayOrder;
    GO
    if exists (select * From sysobjects where id = object_id('DistributorDisplayOrder') and type = 'u')
        drop table dbo.DistributorDisplayOrder;
    GO
    if exists (select * From sysobjects where id = object_id('LibraryDisplayOrder') and type = 'u')
        drop table dbo.LibraryDisplayOrder;
    GO
    if exists (select * From sysobjects where id = object_id('SectionDisplayOrder') and type = 'u')
        drop table dbo.SectionDisplayOrder;
    GO
    if exists (select * From sysobjects where id = object_id('ManufacturerDisplayOrder') and type = 'u')
        drop table dbo.ManufacturerDisplayOrder;
    GO






    update [dbo].category set XmlPackage='entity.grid.xml.config' where XmlPackage IS NULL or XmlPackage='';
    go
    update [dbo].section set XmlPackage='entity.grid.xml.config' where XmlPackage IS NULL or XmlPackage='';
    go
    update [dbo].library set XmlPackage='entity.grid.xml.config' where XmlPackage IS NULL or XmlPackage='';
    go
    update [dbo].manufacturer set XmlPackage='entity.grid.xml.config' where XmlPackage IS NULL or XmlPackage='';
    go
    update [dbo].distributor set XmlPackage='entity.grid.xml.config' where XmlPackage IS NULL or XmlPackage='';
    go
    update [dbo].product set XmlPackage='product.rightbar.xml.config' where XmlPackage IS NULL or XmlPackage='';
    go
    update [dbo].document set XmlPackage='document.default.xml.config' where XmlPackage IS NULL or XmlPackage='';
    go

    update [dbo].ProductVariant set IsTaxable=0 where ProductID in (select ProductID from Product where SKU='GIFTCARD');
    go





    -- DROP ALL LastUpdated and LastUpdatedBy fields
    declare @constrainttable table (f1 varchar(2000))
    declare  @cmd varchar(1000)
    insert @constrainttable 
    select 'ALTER TABLE ' + so2.name + ' DROP CONSTRAINT ' + so.name 
    From dbo.sysobjects so join [dbo].syscolumns sc on so.id = sc.cdefault 
        join [dbo].sysobjects so2 on so2.id = sc.id 
    where sc.name in ('LastUpdated', 'LastUpdatedBy') and so2.name <> 'currency'
    order by so2.name 
    select top 1 @cmd = f1 from @constrainttable 
    while @@rowcount > 0 begin
        --PRINT @cmd
        exec (@cmd)
        delete @constrainttable where f1 = @cmd 
        select top 1 @cmd = f1 from @constrainttable 
    end

    GO


    declare @fieldtable table (f1 varchar(2000))
    declare  @cmd varchar(1000)
    insert @fieldtable 
    select 'ALTER TABLE ' + so.name + ' DROP column ' + sc.name 
    From dbo.sysobjects so join [dbo].syscolumns sc on so.id = sc.id
    where sc.name in ('LastUpdated', 'LastUpdatedBy') and so.name <> 'currency'
    order by so.name 
    select top 1 @cmd = f1 from @fieldtable 
    while @@rowcount > 0 begin
        --PRINT @cmd
        exec (@cmd)
        delete @fieldtable where f1 = @cmd 
        select top 1 @cmd = f1 from @fieldtable 
    end

    GO
    -- end drop all LastUpdated and LastUpdatedBy fields


    




    if not exists (select * from dbo.Feed where name = 'GoogleBase')
        insert [dbo].[Feed](Name,XmlPackage,CanAutoFTP,FTPUsername, FTPPassword, FTPServer,FTPPort, FTPFilename) values('GoogleBase','feed.googlebase.xml.config',1,'', '', 'uploads.google.com',21, '');
    go

    if not exists (select * from dbo.feed where name = 'GoogleBase' and FTPServer = 'uploads.google.com' )
        update dbo.feed set FTPServer = 'uploads.google.com' where name = 'GoogleBase'





    if not exists(select * from dbo.ShippingCalculation where ShippingCalculationID = 10) begin
        set IDENTITY_INSERT [dbo].ShippingCalculation ON;
        INSERT [dbo].ShippingCalculation(ShippingCalculationID,Name,Selected) values(10,'Calculate Shipping By Total & Percent',0);
        set IDENTITY_INSERT [dbo].ShippingCalculation OFF;
    end
    go

    if not exists(select * from dbo.TaxClass where TaxClassID = 1) begin
        set IDENTITY_INSERT dbo.TaxClass ON;
        INSERT [dbo].TaxClass(TaxClassID,Name,TaxCode) values(1,'Goods','');
        INSERT [dbo].TaxClass(TaxClassID,Name,TaxCode) values(2,'Services','');
        INSERT [dbo].TaxClass(TaxClassID,Name,TaxCode) values(3,'Shipping','');
        INSERT [dbo].TaxClass(TaxClassID,Name,TaxCode) values(4,'Tobacco','');
        INSERT [dbo].TaxClass(TaxClassID,Name,TaxCode) values(5,'Alcohol','');
        set IDENTITY_INSERT dbo.TaxClass OFF;
    end
    go

    -- get rid of old data:
    delete from failedtransaction;
    go




    -- make sure these states are mapped to the United States country, this is now required for states/provinces to be mapped to a country in 7.0:
    if exists(select * from dbo.Country where name = 'United States') begin
    declare @cid int
    select @cid=countryid from country where name='United States'
    UPDATE [dbo].State set CountryID=@cid where name='Alabama';
    UPDATE [dbo].State set CountryID=@cid where name='Alaska';
    UPDATE [dbo].State set CountryID=@cid where name='American Samoa';
    UPDATE [dbo].State set CountryID=@cid where name='Armed Forces Americas (except Canada)';
    UPDATE [dbo].State set CountryID=@cid where name='Armed Forces Eur., Mid. East, Africa, Canada';
    UPDATE [dbo].State set CountryID=@cid where name='Armed Forces Pacific';
    UPDATE [dbo].State set CountryID=@cid where name='Arizona';
    UPDATE [dbo].State set CountryID=@cid where name='Arkansas';
    UPDATE [dbo].State set CountryID=@cid where name='California';
    UPDATE [dbo].State set CountryID=@cid where name='Colorado';
    UPDATE [dbo].State set CountryID=@cid where name='Connecticut';
    UPDATE [dbo].State set CountryID=@cid where name='Delaware';
    UPDATE [dbo].State set CountryID=@cid where name='District of Columbia';
    UPDATE [dbo].State set CountryID=@cid where name='Federated States of Micronesia';
    UPDATE [dbo].State set CountryID=@cid where name='Florida';
    UPDATE [dbo].State set CountryID=@cid where name='Georgia';
    UPDATE [dbo].State set CountryID=@cid where name='Guam';
    UPDATE [dbo].State set CountryID=@cid where name='Hawaii';
    UPDATE [dbo].State set CountryID=@cid where name='Idaho';
    UPDATE [dbo].State set CountryID=@cid where name='Illinois';
    UPDATE [dbo].State set CountryID=@cid where name='Indiana';
    UPDATE [dbo].State set CountryID=@cid where name='Iowa';
    UPDATE [dbo].State set CountryID=@cid where name='Kansas';
    UPDATE [dbo].State set CountryID=@cid where name='Kentucky';
    UPDATE [dbo].State set CountryID=@cid where name='Louisiana';
    UPDATE [dbo].State set CountryID=@cid where name='Maine';
    UPDATE [dbo].State set CountryID=@cid where name='Marshall Islands';
    UPDATE [dbo].State set CountryID=@cid where name='Maryland';
    UPDATE [dbo].State set CountryID=@cid where name='Massachusetts';
    UPDATE [dbo].State set CountryID=@cid where name='Michigan';
    UPDATE [dbo].State set CountryID=@cid where name='Minnesota';
    UPDATE [dbo].State set CountryID=@cid where name='Mississippi';
    UPDATE [dbo].State set CountryID=@cid where name='Missouri';
    UPDATE [dbo].State set CountryID=@cid where name='Montana';
    UPDATE [dbo].State set CountryID=@cid where name='Northern Mariana Islands';
    UPDATE [dbo].State set CountryID=@cid where name='Nebraska';
    UPDATE [dbo].State set CountryID=@cid where name='Nevada';
    UPDATE [dbo].State set CountryID=@cid where name='New Hampshire';
    UPDATE [dbo].State set CountryID=@cid where name='New Jersey';
    UPDATE [dbo].State set CountryID=@cid where name='New Mexico';
    UPDATE [dbo].State set CountryID=@cid where name='New York';
    UPDATE [dbo].State set CountryID=@cid where name='North Carolina';
    UPDATE [dbo].State set CountryID=@cid where name='North Dakota';
    UPDATE [dbo].State set CountryID=@cid where name='Ohio';
    UPDATE [dbo].State set CountryID=@cid where name='Oklahoma';
    UPDATE [dbo].State set CountryID=@cid where name='Oregon';
    UPDATE [dbo].State set CountryID=@cid where name='Palau';
    UPDATE [dbo].State set CountryID=@cid where name='Pennsylvania';
    UPDATE [dbo].State set CountryID=@cid where name='Puerto Rico';
    UPDATE [dbo].State set CountryID=@cid where name='Rhode Island';
    UPDATE [dbo].State set CountryID=@cid where name='South Carolina';
    UPDATE [dbo].State set CountryID=@cid where name='South Dakota';
    UPDATE [dbo].State set CountryID=@cid where name='Tennessee';
    UPDATE [dbo].State set CountryID=@cid where name='Texas';
    UPDATE [dbo].State set CountryID=@cid where name='Utah';
    UPDATE [dbo].State set CountryID=@cid where name='Vermont';
    UPDATE [dbo].State set CountryID=@cid where name='Virgin Islands';
    UPDATE [dbo].State set CountryID=@cid where name='Virginia';
    UPDATE [dbo].State set CountryID=@cid where name='Washington';
    UPDATE [dbo].State set CountryID=@cid where name='West Virginia';
    UPDATE [dbo].State set CountryID=@cid where name='Wisconsin';
    UPDATE [dbo].State set CountryID=@cid where name='Wyoming';
    end
    go

    if exists(select * from dbo.Country where name = 'Canada') begin
    declare @cid int
    select @cid=countryid from country where name='Canada'
    UPDATE [dbo].State set CountryID=@cid where name='Alberta';
    UPDATE [dbo].State set CountryID=@cid where name='British Columbia';
    UPDATE [dbo].State set CountryID=@cid where name='Manitoba';
    UPDATE [dbo].State set CountryID=@cid where name='New Brunswick';
    UPDATE [dbo].State set CountryID=@cid where name='Newfoundland and Labrador';
    UPDATE [dbo].State set CountryID=@cid where name='Northwest Territories';
    UPDATE [dbo].State set CountryID=@cid where name='Nova Scotia';
    UPDATE [dbo].State set CountryID=@cid where name='Nunavut';
    UPDATE [dbo].State set CountryID=@cid where name='Ontario';
    UPDATE [dbo].State set CountryID=@cid where name='Prince Edward Island';
    UPDATE [dbo].State set CountryID=@cid where name='Quebec';
    UPDATE [dbo].State set CountryID=@cid where name='Saskatchewan';
    UPDATE [dbo].State set CountryID=@cid where name='Yukon Territory';
    end
    go


    update [dbo].State set Name = 'AA (Armed Forces Americas)' where CountryID = 1 and abbreviation = 'AA'
    update [dbo].State set Name = 'AE (Armed Forces Europe)'   where CountryID = 1 and abbreviation = 'AE'
    update [dbo].State set Name = 'AP (Armed Forces Pacific)'  where CountryID = 1 and abbreviation = 'AP'
    go



    if not exists (select * from [dbo].State where Abbreviation = '--')
        INSERT [dbo].State(CountryID,Name,Abbreviation,DisplayOrder) values(null,'Other (Non U.S.)','--',1);
    go



    delete dbo.ShippingMethodToCountryMap from dbo.ShippingMethodToCountryMap scm join dbo.shippingmethod sm on scm.ShippingMethodID = sm.ShippingMethodID where sm.IsRTShipping = 1
    delete dbo.ShippingMethodToStateMap from dbo.ShippingMethodToStateMap ssm join dbo.shippingmethod sm on ssm.ShippingMethodID = sm.ShippingMethodID where sm.IsRTShipping = 1
    delete dbo.ShippingMethodToZoneMap  from dbo.ShippingMethodToZoneMap szm join dbo.shippingmethod sm on szm.ShippingMethodID = sm.ShippingMethodID where sm.IsRTShipping = 1


    if exists (select * From syscolumns where id = object_id('FailedTransaction') and name = 'MaxMindFraudScore') 
        update dbo.FailedTransaction set MaxMindFraudScore = -1 where MaxMindFraudScore is null
    go

    if exists (select * From syscolumns where id = object_id('Orders') and name = 'MaxMindFraudScore') 
        update dbo.Orders set MaxMindFraudScore = -1 where MaxMindFraudScore is null
    go



    --  Default EventHandlers, inactive by default
    if not exists (select * From dbo.Eventhandler where EventName = 'ViewProductPage') 
        INSERT dbo.Eventhandler (EventName, CalloutURL, XmlPackage, Active) VALUES ('ViewProductPage', '', 'event.default.xml.config', 0);
    if not exists (select * From dbo.Eventhandler where EventName = 'ViewEntityPage') 
        INSERT dbo.Eventhandler (EventName, CalloutURL, XmlPackage, Active) VALUES ('ViewEntityPage', '', 'event.default.xml.config', 0);
    if not exists (select * From dbo.Eventhandler where EventName = 'AddToCart') 
        INSERT dbo.Eventhandler (EventName, CalloutURL, XmlPackage, Active) VALUES ('AddToCart', '', 'event.default.xml.config', 0);
    if not exists (select * From dbo.Eventhandler where EventName = 'BeginCheckout') 
        INSERT dbo.Eventhandler (EventName, CalloutURL, XmlPackage, Active) VALUES ('BeginCheckout', '', 'event.default.xml.config', 0);
    if not exists (select * From dbo.Eventhandler where EventName = 'RemoveFromCart') 
        INSERT dbo.Eventhandler (EventName, CalloutURL, XmlPackage, Active) VALUES ('RemoveFromCart', '', 'event.default.xml.config', 0);
    if not exists (select * From dbo.Eventhandler where EventName = 'CreateCustomer') 
        INSERT dbo.Eventhandler (EventName, CalloutURL, XmlPackage, Active) VALUES ('CreateCustomer', '', 'event.default.xml.config', 0);
    if not exists (select * From dbo.Eventhandler where EventName = 'UpdateCustomer') 
        INSERT dbo.Eventhandler (EventName, CalloutURL, XmlPackage, Active) VALUES ('UpdateCustomer', '', 'event.default.xml.config', 0);
    if not exists (select * From dbo.Eventhandler where EventName = 'DeleteCustomer') 
        INSERT dbo.Eventhandler (EventName, CalloutURL, XmlPackage, Active) VALUES ('DeleteCustomer', '', 'event.default.xml.config', 0);
    if not exists (select * From dbo.Eventhandler where EventName = 'NukeCustomer') 
        INSERT dbo.Eventhandler (EventName, CalloutURL, XmlPackage, Active) VALUES ('NukeCustomer', '', 'event.default.xml.config', 0);
    if not exists (select * From dbo.Eventhandler where EventName = 'CreateAccount') 
        INSERT dbo.Eventhandler (EventName, CalloutURL, XmlPackage, Active) VALUES ('CreateAccount', '', 'event.default.xml.config', 0);
    if not exists (select * From dbo.Eventhandler where EventName = 'CheckoutShipping') 
        INSERT dbo.Eventhandler (EventName, CalloutURL, XmlPackage, Active) VALUES ('CheckoutShipping', '', 'event.default.xml.config', 0);
    if not exists (select * From dbo.Eventhandler where EventName = 'CheckoutPayment') 
        INSERT dbo.Eventhandler (EventName, CalloutURL, XmlPackage, Active) VALUES ('CheckoutPayment', '', 'event.default.xml.config', 0);
    if not exists (select * From dbo.Eventhandler where EventName = 'CheckoutReview') 
        INSERT dbo.Eventhandler (EventName, CalloutURL, XmlPackage, Active) VALUES ('CheckoutReview', '', 'event.default.xml.config', 0);
    if not exists (select * From dbo.Eventhandler where EventName = 'NewOrder') 
        INSERT dbo.Eventhandler (EventName, CalloutURL, XmlPackage, Active) VALUES ('NewOrder', '', 'event.default.xml.config', 0);
    if not exists (select * From dbo.Eventhandler where EventName = 'OrderDeleted') 
        INSERT dbo.Eventhandler (EventName, CalloutURL, XmlPackage, Active) VALUES ('OrderDeleted', '', 'event.default.xml.config', 0);
    if not exists (select * From dbo.Eventhandler where EventName = 'OrderVoided') 
        INSERT dbo.Eventhandler (EventName, CalloutURL, XmlPackage, Active) VALUES ('OrderVoided', '', 'event.default.xml.config', 0);
    if not exists (select * From dbo.Eventhandler where EventName = 'OrderShipped') 
        INSERT dbo.Eventhandler (EventName, CalloutURL, XmlPackage, Active) VALUES ('OrderShipped', '', 'event.default.xml.config', 0);
    if not exists (select * From dbo.Eventhandler where EventName = 'OrderRefunded') 
        INSERT dbo.Eventhandler (EventName, CalloutURL, XmlPackage, Active) VALUES ('OrderRefunded', '', 'event.default.xml.config', 0);




    IF exists (select * from sysobjects where type = 'u' and name = 'RecurringShipments')
        DROP TABLE dbo.RecurringShipments
    GO
    IF exists (select * from sysobjects where type = 'u' and name = 'Expense')
        DROP TABLE dbo.Expense
    GO
    IF exists (select * from sysobjects where type = 'u' and name = 'ExpenseCategory')
        DROP TABLE dbo.ExpenseCategory
    GO
    IF exists (select * from sysobjects where type = 'u' and name = 'RefundHistory')
        DROP TABLE dbo.RefundHistory
    GO

    if exists (select * From sysobjects where id = object_id('FailedAdminLoginAttempts')) begin
        drop table dbo.FailedAdminLoginAttempts
    end
    go

    if exists (select * From sysobjects where id = object_id('FraudOrders')) begin
        drop table dbo.FraudOrders
    end
    go

    if exists (select * From sysobjects where id = object_id('FraudOrders_CustomCart')) begin
        drop table dbo.FraudOrders_CustomCart
    end
    go

    if exists (select * From sysobjects where id = object_id('FraudOrders_KitCart')) begin
        drop table dbo.FraudOrders_KitCart
    end
    go

    if exists (select * From sysobjects where id = object_id('FraudOrders_ShoppingCart')) begin
        drop table dbo.FraudOrders_ShoppingCart
    end
    go




    if exists (select * From sysobjects where id = object_id('SM_FEDEX') and type = 'u') begin
        INSERT INTO [ShippingImportExport]([OrderNumber],[CustomerID],[CompanyName],[CustomerLastName],[CustomerFirstName],[CustomerPhone],[CustomerEmail],[Address1],[Address2],[Suite],[City],[State],[Zip],[Country],[ServiceCarrierCode],[TrackingNumber],[Cost],[Weight])
        SELECT [OrderNumber],[CustomerID],[CompanyName],[CustomerLastName],[CustomerFirstName],[CustomerPhone],[CustomerEmail],[Address1],[Address2],[Suite],[City],[State],[Zip],[Country],[ServiceCarrierCode],			[FedExTrackingNumber],[FedExCost],[FedExWeight]
        FROM SM_FEDEX

        DROP TABLE dbo.SM_FEDEX
    end

    GO




    update dbo.AppConfig set ConfigValue='8.0.1.2' where name='StoreVersion';
