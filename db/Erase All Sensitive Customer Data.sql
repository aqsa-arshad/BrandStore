-- ------------------------------------------------------------------------------------------------------
-- ------------------------------------------------------------------------------------------
-- Copyright AspDotNetStorefront.com.  All Rights Reserved.
-- http://www.aspdotnetstorefront.com
-- For details on this license please visit  the product homepage at the URL above.
-- THE ABOVE NOTICE MUST REMAIN INTACT.
-- ------------------------------------------------------------------------------------------
--
-- this script cleans the active db of all customer data script!
-- this script can be used if you take a production database backup to 
-- store locally and test product configurations, etc. this should remove any
-- sensitive customer data
--
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- WARNING: FOR DEVELOPMENT USE ONLY. DO NOT USE ON A PRODUCTION DATABASE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- ------------------------------------------------------------------------------------------------------

set nocount on 

update customer set password='test';
go

truncate table fraudorders;
go
truncate table fraudorders_shoppingcart;
go
truncate table fraudorders_customcart;
go
truncate table fraudorders_kitcart;
go

update orders set AuthorizationResult=NULL, AuthorizationPNREF=NULL, echeckBankAccountNumber=NULL, echeckBankABACode=NULL;
go

-- clean orders table
update [dbo].orders set Password=NULL, CardNumber=NULL, TransactionCommand=NULL, RefundTXCommand=NULL, RefundTXResult=NULL, VoidTXCommand=NULL, VoidTXResult=NULL, CaptureTXCommand=NULL, CaptureTXResult=NULL;
go
IF exists (select * From dbo.sysobjects so with (nolock) join [dbo].syscolumns sc on so.id = sc.id where so.type= 'u' and so.name = 'orders' and sc.name = 'CardExtraCode') BEGIN
	DECLARE @cmd varchar(8000)
	SET @cmd = 'update [dbo].Orders SET CardExtraCode = null'
	EXEC (@cmd)
	SET @cmd = ''
END 
go

-- wipe out any address CC #'s
DECLARE @cmd varchar(8000)
IF exists (select * from dbo.sysobjects with (nolock) where type = 'u' and name = 'address')
	IF exists (select * from dbo.syscolumns with (nolock) where id = object_id('address') and name = 'CardExtraCode')
	BEGIN
		SET @cmd = 'update [dbo].address set echeckBankABACode=NULL, echeckBankAccountNumber=NULL, CardNumber=NULL, CardExtraCode=NULL,CardExpirationMonth=NULL, CardExpirationYear=NULL where CustomerID not in (select CustomerID from ShoppingCart where CartType=2)'
		EXEC (@cmd)
		SET @cmd = ''
	END 
	ELSE
	BEGIN
		SET @cmd = 'update [dbo].address set echeckBankABACode=NULL, echeckBankAccountNumber=NULL, CardNumber=NULL, CardExpirationMonth=NULL, CardExpirationYear=NULL where CustomerID not in (select CustomerID from ShoppingCart where CartType=2)'
		EXEC (@cmd)
		SET @cmd = ''
	END 
ELSE
BEGIN
	-- erase credit card info from all customer records (recurring orders were not supported in these old versions)
	IF exists (select * From sysobjects so with (nolock) join [dbo].syscolumns sc on so.id = sc.id where so.type= 'u' and so.name = 'customer' and sc.name = 'CardNumber') BEGIN
		SET @cmd = 'update [dbo].Customer SET CardNumber = null'
		EXEC (@cmd)
		SET @cmd = ''
	END 
END
go

-- wipe out admin data:
update [dbo].customer set EMail='admin@store.com', Password='admin' where IsAdmin=1;
go

-- erase logged sql statements
DELETE dbo.SQLLog
go

-- clear customer session table (only useful for server farms), always done:
truncate table CustomerSession;
go

-- randomize all customer cookies/guids:
update [dbo].customer set CustomerGUID=newid();
go

-- purge anon customers:
delete dbo.customer where EMail like 'Anon_%' 
        and customerid not in (select customerid from dbo.ShoppingCart with (NOLOCK)) 
        and customerid not in (select customerid from dbo.kitcart with (NOLOCK)) 
        and customerid not in (select customerid from dbo.customcart with (NOLOCK)) 
        and customerid not in (select customerid from dbo.orders with (NOLOCK)) 
        and customerid not in (select customerid from dbo.rating with (NOLOCK)) 
        and customerid not in (select ratingcustomerid from dbo.ratingcommenthelpfulness with (NOLOCK)) 
        and customerid not in (select votingcustomerid from dbo.ratingcommenthelpfulness with (NOLOCK)) 
        and customerid not in (select customerid from dbo.pollvotingrecord with (NOLOCK));
go

truncate table failedtransaction;
go

-- clean any orphaned addresses
delete Address where CustomerID Not in (select CustomerID from customer with (NOLOCK));
go

