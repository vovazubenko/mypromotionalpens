IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FNS_SpeedFilter_GetSpeedFilters]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[FNS_SpeedFilter_GetSpeedFilters]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FNS_SpeedFilter_ProductLoadAllPaged_GetMinimumPrice]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[FNS_SpeedFilter_ProductLoadAllPaged_GetMinimumPrice]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fns_splitstring_group_to_table]') AND type in (N'F', N'TF'))
DROP FUNCTION [dbo].[fns_splitstring_group_to_table]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FNS_SpeedFilter_ProductLoadAllPaged]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[FNS_SpeedFilter_ProductLoadAllPaged]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FNS_SpeedFilter_Authorize_ById]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[FNS_SpeedFilter_Authorize_ById]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GenerateFilterUrlFromSeoUrl]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GenerateFilterUrlFromSeoUrl]
GO
