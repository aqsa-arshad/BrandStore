-- ------------------------------------------------------------------------------------------
-- Copyright AspDotNetStorefront.com.  All Rights Reserved.
-- http://www.aspdotnetstorefront.com
-- For details on this license please visit  the product homepage at the URL above.
-- THE ABOVE NOTICE MUST REMAIN INTACT.
-- ------------------------------------------------------------------------------------------
-- generates list of sql statements you can execute to force all non dbo tables to be moved back to dbo ownership:
SELECT 'EXEC(''sp_changeobjectowner @objname = ''''' +  ltrim(u.name) + '.' + ltrim(s.name) + ''''''   + ', @newowner = dbo'')' FROM  sysobjects s, sysusers u WHERE s.uid = u.uid AND   u.name <> 'dbo'AND   xtype in ('V', 'P', 'U')AND   u.name not like 'INFORMATION%' order by s.name