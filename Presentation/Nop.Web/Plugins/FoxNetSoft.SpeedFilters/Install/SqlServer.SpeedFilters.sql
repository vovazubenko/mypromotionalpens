﻿IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FNS_SpeedFilter_GetSpeedFilters]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[FNS_SpeedFilter_GetSpeedFilters]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[FNS_SpeedFilter_GetSpeedFilters]
(
	@categoryId int,    --Category identifier
	@manufacturerId int,   --Manufacturer identifier
	@vendorId int ,	--Vendor identifier
	@storeId			int, -- Store mapping
	@languageId			int, -- Language  identifier
	@ShowProductsFromSubcategories bit, -- Show Products From Subcategories
	@AllowedCustomerRoleIds	nvarchar(MAX),	--a list of customer role IDs (comma-separated list) for which a product should be shown (if a subjet to ACL)
	@enablePriceRangeFilter bit,  -- enable Price Range Filter
	@enableSpecificationsFilter bit, -- enable Specifications Filter
	@enableAttributesFilter bit, -- enable Attributes Filter
	@enableManufacturersFilter bit, -- enable Manufacturers Filter
	@enableVendorsFilter bit, -- enable Vendors Filter
	@Keywords			nvarchar(4000) = null,
	@SearchDescriptions bit = 0, --a value indicating whether to search by a specified "keyword" in product descriptions
	@UseFullTextSearch  bit = 0,
	@FullTextMode		int = 0, --0 - using CONTAINS with <prefix_term>, 5 - using CONTAINS and OR with <prefix_term>, 10 - using CONTAINS and AND with <prefix_term>
	@PriceMin			decimal(18, 4) = null,
	@PriceMax			decimal(18, 4) = null
)
AS
BEGIN
SET NOCOUNT ON
--Gets Products for only read.

declare @countRec int
set @countRec=0
if @languageId>0
begin
	select @countRec=count(*) from [Language] WITH (NOLOCK)
	if @countRec=1
		set @languageId=0
end
if @storeId>0
begin
	set @countRec=0
	select @countRec=count(*) from [Store] WITH (NOLOCK)
	if @countRec=1
		set @storeId=0
end

create table #ProductID (Id int,SubjectToAcl bit,LimitedToStores bit)

--filter by customer role IDs (access control list)
SET @AllowedCustomerRoleIds = isnull(@AllowedCustomerRoleIds, '')	
CREATE TABLE #FilteredCustomerRoleIds
(
	CustomerRoleId int not null
)
INSERT INTO #FilteredCustomerRoleIds (CustomerRoleId)
SELECT CAST(data as int) FROM [nop_splitstring_to_table](@AllowedCustomerRoleIds, ',')	

DECLARE
	@SearchKeywords bit,
	@sql nvarchar(max)

	--filter by keywords
	SET @Keywords = isnull(@Keywords, '')
	SET @Keywords = rtrim(ltrim(@Keywords))
	SET @SearchKeywords=0
	IF ISNULL(@Keywords, '') != ''
	BEGIN
		SET @SearchKeywords = 1
		
		IF @UseFullTextSearch = 1
		BEGIN
			--remove wrong chars (' ")
			SET @Keywords = REPLACE(@Keywords, '''', '')
			SET @Keywords = REPLACE(@Keywords, '"', '')
			
			--full-text search
			IF @FullTextMode = 0 
			BEGIN
				--0 - using CONTAINS with <prefix_term>
				SET @Keywords = ' "' + @Keywords + '*" '
			END
			ELSE
			BEGIN
				--5 - using CONTAINS and OR with <prefix_term>
				--10 - using CONTAINS and AND with <prefix_term>

				--clean multiple spaces
				WHILE CHARINDEX('  ', @Keywords) > 0 
					SET @Keywords = REPLACE(@Keywords, '  ', ' ')

				DECLARE @concat_term nvarchar(100)				
				IF @FullTextMode = 5 --5 - using CONTAINS and OR with <prefix_term>
				BEGIN
					SET @concat_term = 'OR'
				END 
				IF @FullTextMode = 10 --10 - using CONTAINS and AND with <prefix_term>
				BEGIN
					SET @concat_term = 'AND'
				END

				--now let's build search string
				declare @fulltext_keywords nvarchar(4000)
				set @fulltext_keywords = N''
				declare @index int		
		
				set @index = CHARINDEX(' ', @Keywords, 0)

				-- if index = 0, then only one field was passed
				IF(@index = 0)
					set @fulltext_keywords = ' "' + @Keywords + '*" '
				ELSE
				BEGIN		
					DECLARE @first BIT
					SET  @first = 1			
					WHILE @index > 0
					BEGIN
						IF (@first = 0)
							SET @fulltext_keywords = @fulltext_keywords + ' ' + @concat_term + ' '
						ELSE
							SET @first = 0

						SET @fulltext_keywords = @fulltext_keywords + '"' + SUBSTRING(@Keywords, 1, @index - 1) + '*"'					
						SET @Keywords = SUBSTRING(@Keywords, @index + 1, LEN(@Keywords) - @index)						
						SET @index = CHARINDEX(' ', @Keywords, 0)
					end
					
					-- add the last field
					IF LEN(@fulltext_keywords) > 0
						SET @fulltext_keywords = @fulltext_keywords + ' ' + @concat_term + ' ' + '"' + SUBSTRING(@Keywords, 1, LEN(@Keywords)) + '*"'	
				END
				SET @Keywords = @fulltext_keywords
			END
		END
		ELSE
		BEGIN
			--usual search by PATINDEX
			SET @Keywords = '%' + @Keywords + '%'
		END
		--PRINT @Keywords
	END

	SET @sql = '
	insert into #ProductID (Id,SubjectToAcl,LimitedToStores)
	select p.Id,p.SubjectToAcl,p.LimitedToStores 
	FROM
		Product p WITH (NOLOCK)
	where p.Deleted=0 and p.Published=1 '

create table #CategoryId (Id int,SubjectToAcl bit,LimitedToStores bit)

if @categoryId>0
begin
	if @ShowProductsFromSubcategories=1
	begin
		--Category tree
		;with cte_tree (Id,ParentCategoryId,LimitedToStores,SubjectToAcl) as 
		(
			select C.Id,C.ParentCategoryId,C.LimitedToStores,C.SubjectToAcl
			from Category C WITH (NOLOCK)
			where C.Id=@categoryId
				and C.Deleted=0 and C.Published=1
			union all
			select T.Id,T.ParentCategoryId,T.LimitedToStores,T.SubjectToAcl
			from cte_tree C,Category T WITH (NOLOCK)
			where C.Id=T.ParentCategoryId and T.Deleted=0 and T.Published=1
		)
		insert into #CategoryId (Id,LimitedToStores,SubjectToAcl)		
		select Id,LimitedToStores,SubjectToAcl
		from cte_tree
	end
	else
	begin
		insert into #CategoryId (Id,LimitedToStores,SubjectToAcl)		
		select C.Id,C.LimitedToStores,C.SubjectToAcl
		from Category C WITH (NOLOCK)
		where C.Id=@categoryId
			and C.Deleted=0 and C.Published=1
	end
	--ACL Category
	if exists(select Id from #CategoryId where SubjectToAcl=1)		
	begin
		delete #CategoryId
		from #CategoryId C
		where C.SubjectToAcl=1 and not EXISTS (
							SELECT 1 FROM #FilteredCustomerRoleIds [fcr]
								WHERE
									[fcr].CustomerRoleId IN (
										SELECT [acl].CustomerRoleId
										FROM [AclRecord] acl  WITH (NOLOCK)
										WHERE [acl].EntityId = C.Id AND [acl].EntityName = 'Category'
									)
							)
	end
	--StoreMapping Category
	--//// Note : ignore for multiple store 
	--if @storeId>0 and exists(select Id from #CategoryId where LimitedToStores=1)		
	--begin
	--	delete #CategoryId
	--	from #CategoryId C
	--	where C.LimitedToStores=1 and not Exists(select Top 1 Id from StoreMapping sm WITH (NOLOCK)
	--			where sm.EntityName = 'Category' and sm.StoreId=@storeId and sm.EntityId=C.Id)
	--end
	
	SET @sql = @sql+ '
		and p.Id in (select PC.ProductId 
			from Product_Category_Mapping PC WITH (NOLOCK) 
				where PC.CategoryId in (select Id from #CategoryId))'
end

if @manufacturerId>0
begin
	SET @sql = @sql+ '
		and p.Id in (select PM.ProductId 
		from Product_Manufacturer_Mapping PM WITH (NOLOCK) 
		where PM.ManufacturerId=' + CAST(@manufacturerId AS nvarchar(max))+')'
end

if @vendorId>0
begin
	SET @sql = @sql+ '
		and p.VendorId=' + CAST(@vendorId AS nvarchar(max))
end

--for search page
IF @SearchKeywords = 1
BEGIN
	SET @sql = @sql + 'AND (
			'
		--product name 
		IF @UseFullTextSearch = 1
			SET @sql = @sql + '
			CONTAINS(p.[Name], @Keywords) '
		ELSE
			SET @sql = @sql + '
			PATINDEX(@Keywords, p.[Name]) > 0 '
		--SKU
		IF @UseFullTextSearch = 1
			SET @sql = @sql + '
			OR CONTAINS(p.[Sku], @Keywords) '
		ELSE
			SET @sql = @sql + '
			OR PATINDEX(@Keywords, p.[Sku]) > 0 '				

		if @LanguageId>0
		begin
			--localized product name
			SET @sql = @sql + ' 
			OR p.Id in (
				SELECT lp.EntityId
				FROM LocalizedProperty lp with (NOLOCK)
				WHERE
					lp.LocaleKeyGroup = N''Product''
					AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '
					AND lp.LocaleKey = N''Name'''
			IF @UseFullTextSearch = 1
				SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '
			ELSE
				SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '
			SET @sql = @sql + ')'	
		end

		IF @SearchDescriptions = 1
		BEGIN
			--product short description
			IF @UseFullTextSearch = 1
				SET @sql = @sql + '
					OR CONTAINS(p.[ShortDescription], @Keywords) '
			ELSE
				SET @sql = @sql + '
					OR PATINDEX(@Keywords, p.[ShortDescription]) > 0 '			

			--product full description
			IF @UseFullTextSearch = 1
				SET @sql = @sql + '
					OR CONTAINS(p.[FullDescription], @Keywords) '
			ELSE
				SET @sql = @sql + '
					OR PATINDEX(@Keywords, p.[FullDescription]) > 0 '

			if @LanguageId>0
			begin
				--localized product short description
				SET @sql = @sql + ' 
						OR p.Id in (
							SELECT lp.EntityId
							FROM LocalizedProperty lp with (NOLOCK)
							WHERE
								lp.LocaleKeyGroup = N''Product''
								AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '
								AND lp.LocaleKey = N''ShortDescription'''
				IF @UseFullTextSearch = 1
					SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '
				ELSE
					SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '
				SET @sql = @sql + ')'	

				--localized product full description
				SET @sql = @sql + ' 
					OR p.Id in (
							SELECT lp.EntityId
							FROM LocalizedProperty lp with (NOLOCK)
							WHERE
								lp.LocaleKeyGroup = N''Product''
								AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '
								AND lp.LocaleKey = N''FullDescription'''
				IF @UseFullTextSearch = 1
					SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '
				ELSE
					SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '
				SET @sql = @sql + ')'
			end

			--product tag
			SET @sql = @sql + ' 
					OR p.Id in (
						SELECT pptm.Product_Id
						FROM Product_ProductTag_Mapping pptm with(NOLOCK) INNER JOIN ProductTag pt with(NOLOCK) ON pt.Id = pptm.ProductTag_Id
						WHERE '
				IF @UseFullTextSearch = 1
					SET @sql = @sql + 'CONTAINS(pt.[Name], @Keywords) '
				ELSE
					SET @sql = @sql + 'PATINDEX(@Keywords, pt.[Name]) > 0 '
				SET @sql = @sql + ')'
			if @LanguageId>0
			begin
				--localized product tag
				SET @sql = @sql + ' 
						OR p.Id in (
							SELECT pptm.Product_Id
							FROM LocalizedProperty lp with (NOLOCK) INNER JOIN Product_ProductTag_Mapping pptm with(NOLOCK) ON lp.EntityId = pptm.ProductTag_Id
							WHERE
								lp.LocaleKeyGroup = N''ProductTag''
								AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '
								AND lp.LocaleKey = N''Name'''
				IF @UseFullTextSearch = 1
					SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '
				ELSE
					SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '
				SET @sql = @sql + ')'
			END
		END
		SET @sql = @sql + '
			)'
END

--min price
IF @PriceMin > 0
BEGIN
	SET @sql = @sql + '
		AND (p.Price >= ' + CAST(@PriceMin AS nvarchar(max)) + ')'
END
	
--max price
IF @PriceMax > 0
BEGIN
	SET @sql = @sql + '
		AND (p.Price <= ' + CAST(@PriceMax AS nvarchar(max)) + ')'
END

--PRINT (@sql)
EXEC sp_executesql @sql, N'@Keywords nvarchar(4000)', @Keywords

drop table #CategoryId

--ACL Product
if exists(select Id from #ProductID where SubjectToAcl=1)		
begin
	delete #ProductID
	from #ProductID C
	where C.SubjectToAcl=1 and not EXISTS (
						SELECT 1 FROM #FilteredCustomerRoleIds [fcr]
							WHERE
								[fcr].CustomerRoleId IN (
									SELECT [acl].CustomerRoleId
									FROM [AclRecord] acl  WITH (NOLOCK)
									WHERE [acl].EntityId = C.Id AND [acl].EntityName = 'Product'
								)
						)
end
--StoreMapping Category

-- // Note : Ignore for multiple store mapping
--if @storeId>0 and exists(select Id from #ProductID where LimitedToStores=1)		
--begin
--	delete #ProductID
--	from #ProductID C
--	where C.LimitedToStores=1 and not Exists(select Top 1 Id from StoreMapping sm WITH (NOLOCK)
--			where sm.EntityName = 'Product' and sm.StoreId=@storeId and sm.EntityId=C.Id)
--end
/*
select * from Product P WITH (NOLOCK)
	where P.Id in (select Id from #ProductID)
	*/
	
/*Get Min and Max Price*/		
if @enablePriceRangeFilter=1
begin
	set @PriceMin=0
	set @PriceMax=0

	select @PriceMin=min(P.Price),
		@PriceMax=max(P.Price)
	from Product P WITH (NOLOCK)
	where P.Id in (select Id from #ProductID)
	
	select isnull(@PriceMin,0) as PriceMin,isnull(@PriceMax,0) as PriceMax 
end	

--SpecificationAttribute
if @enableSpecificationsFilter=1
begin
	create table #tmpSpecificationAttributeOption (Id int not null, SpecificationAttributeId int NOT NULL)

	insert into #tmpSpecificationAttributeOption (Id, SpecificationAttributeId)
	select SO.Id,SO.SpecificationAttributeId 
	from SpecificationAttributeOption SO WITH (NOLOCK)
	where SO.Id in (
		select distinct PSM.SpecificationAttributeOptionId
		from Product_SpecificationAttribute_Mapping PSM WITH (NOLOCK)
		WHERE PSM.ProductId in (select Id from #ProductID)
			and PSM.AllowFiltering=1
		)

	if @LanguageId>0
	begin
		--many languages
		select S.Id,ISNULL(L.LocaleValue,S.Name) as Name,S.DisplayOrder
		from SpecificationAttribute S WITH (NOLOCK)
			left join LocalizedProperty L on S.Id=L.EntityId 
					and L.LocaleKeyGroup='SpecificationAttribute' 
					and L.LocaleKey='Name' 
					and L.LanguageId=@LanguageId	
		where S.Id in (select SpecificationAttributeId from #tmpSpecificationAttributeOption)
		order by S.DisplayOrder

		select SO.Id,SO.SpecificationAttributeId,ISNULL(L.LocaleValue,SO.Name) as Name,SO.DisplayOrder
		from SpecificationAttributeOption SO WITH (NOLOCK)
					left join LocalizedProperty L on SO.Id=L.EntityId 
					and L.LocaleKeyGroup='SpecificationAttributeOption' 
					and L.LocaleKey='Name' 
					and L.LanguageId=@LanguageId	
		where SO.Id in (select Id from #tmpSpecificationAttributeOption)
		order by SO.Name,SO.SpecificationAttributeId,SO.DisplayOrder
	end
	else
	begin
		--one language
		select S.Id,S.Name,S.DisplayOrder
		from SpecificationAttribute S WITH (NOLOCK)
		where S.Id in (select SpecificationAttributeId from #tmpSpecificationAttributeOption)
		order by S.DisplayOrder

		select SO.Id,SO.SpecificationAttributeId,SO.Name,SO.DisplayOrder
		from SpecificationAttributeOption SO WITH (NOLOCK)
		where SO.Id in (select Id from #tmpSpecificationAttributeOption)
		order by SO.Name,SO.SpecificationAttributeId,SO.DisplayOrder
	end 

	drop table #tmpSpecificationAttributeOption
end

--AttributesFilter
if @enableAttributesFilter=1
begin
	create table #tmpAttributesFilter (Id int not null, ProductAttributeId int NOT NULL)
	insert into #tmpAttributesFilter (Id, ProductAttributeId)
	select PAM.Id, PAM.ProductAttributeId
	from Product_ProductAttribute_Mapping PAM WITH (NOLOCK)
	WHERE PAM.ProductId in (select Id from #ProductID)
		and PAM.AttributeControlTypeId in (1,2,3,40,50)
	
	if @LanguageId>0
	begin
		--many languages
		select PA.Id,ISNULL(L.LocaleValue,PA.Name) as Name
		from ProductAttribute PA WITH (NOLOCK)
			left join LocalizedProperty L on PA.Id=L.EntityId 
				and L.LocaleKeyGroup='ProductAttribute' 
				and L.LocaleKey='Name' 
				and L.LanguageId=@LanguageId	
		where PA.Id in (select ProductAttributeId from #tmpAttributesFilter)
		order by PA.Name

		select PA.ProductAttributeId,ISNULL(L.LocaleValue,PVA.Name) as Name,MIN(PVA.Id) as Id 
		from ProductAttributeValue PVA WITH (NOLOCK)
			left join LocalizedProperty L on PVA.Id=L.EntityId 
				and L.LocaleKeyGroup='ProductAttributeValue' 
				and L.LocaleKey='Name' 
				and L.LanguageId=@LanguageId	
			,#tmpAttributesFilter PA
		where PVA.ProductAttributeMappingId=PA.Id
		Group by PA.ProductAttributeId,ISNULL(L.LocaleValue,PVA.Name)
		order by 1,2
	end
	else
	begin
		--one language
		select PA.Id,PA.Name 
		from ProductAttribute PA WITH (NOLOCK)
		where PA.Id in (select ProductAttributeId from #tmpAttributesFilter)
		order by PA.Name

		select PA.ProductAttributeId,PVA.Name,MIN(PVA.Id) as Id 
		from ProductAttributeValue PVA WITH (NOLOCK),#tmpAttributesFilter PA
		where PVA.ProductAttributeMappingId=PA.Id
		Group by PA.ProductAttributeId,PVA.Name
		order by 1,2
	end 
	drop table #tmpAttributesFilter
end

--ManufacturersFilter
if @enableManufacturersFilter=1
begin
	create table #tmpManufacturer (Id int not null, SubjectToAcl bit NOT NULL,LimitedToStores bit not null)
	insert into #tmpManufacturer (Id, SubjectToAcl, LimitedToStores)
	select M.Id,M.SubjectToAcl,M.LimitedToStores 
	from Manufacturer M WITH (NOLOCK)
	where M.Id in (select PM.ManufacturerId
		from Product_Manufacturer_Mapping PM WITH (NOLOCK) 
			where PM.ProductId in (select Id from #ProductID))
		and M.Deleted=0 and M.Published=1
	
	
	  --ACL Manufacturer
		if exists(select Id from #tmpManufacturer where SubjectToAcl=1)		
		begin
			delete #tmpManufacturer
			from #tmpManufacturer C
			where C.SubjectToAcl=1 and not EXISTS (
								SELECT 1 FROM #FilteredCustomerRoleIds [fcr]
									WHERE
										[fcr].CustomerRoleId IN (
											SELECT [acl].CustomerRoleId
											FROM [AclRecord] acl  WITH (NOLOCK)
											WHERE [acl].EntityId = C.Id AND [acl].EntityName = 'Manufacturer'
										)
								)
		end
		--StoreMapping Manufacturer

		--if @storeId>0 and exists(select Id from #tmpManufacturer where LimitedToStores=1)		
		--begin
		--	delete #tmpManufacturer
		--	from #tmpManufacturer C
		--	where C.LimitedToStores=1 and not Exists(select Top 1 Id from StoreMapping sm WITH (NOLOCK)
		--			where sm.EntityName = 'Manufacturer' and sm.StoreId=@storeId and sm.EntityId=C.Id)
		--end

	if @LanguageId>0
	begin
		--many languages
		select M.Id,ISNULL(L.LocaleValue,M.name) as name,M.DisplayOrder
		from Manufacturer M WITH (NOLOCK)
			left join LocalizedProperty L on M.Id=L.EntityId 
				and L.LocaleKeyGroup='Manufacturer' 
				and L.LocaleKey='Name' 
				and L.LanguageId=@LanguageId	
		where M.Id in (select Id from #tmpManufacturer)
		order by M.DisplayOrder
	end
	else
	begin
		--one language
		select M.Id,M.name,M.DisplayOrder
		from Manufacturer M WITH (NOLOCK)
		where M.Id in (select Id from #tmpManufacturer)
		order by M.DisplayOrder
	end 
	drop table #tmpManufacturer	
end

--VendorsFilter
if @enableVendorsFilter=1
begin
	select V.Id,V.name
	from Vendor V WITH (NOLOCK)
	where V.Id in (select P.VendorId
		from Product P WITH (NOLOCK) 
			where P.Id in (select Id from #ProductID))
		and V.Deleted=0
	order by V.Name
end

drop table #FilteredCustomerRoleIds
drop table #ProductID
 
END
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FNS_SpeedFilter_ProductLoadAllPaged_GetMinimumPrice]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[FNS_SpeedFilter_ProductLoadAllPaged_GetMinimumPrice]
GO

/****** Object:  StoredProcedure [dbo].[FNS_Catalog_ProductLoadAllPaged_GetMinimumPrice]    Script Date: 04/30/2014 09:29:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[FNS_SpeedFilter_ProductLoadAllPaged_GetMinimumPrice]
(
	@StoreId			int = 0,
	@grouped bit =0
)
AS
BEGIN
SET NOCOUNT ON
--#ProductId (Id, ParentGroupedProductId, Price, HasTierPrices)

--TierPrice
create table #ProductsTierPrice (Id int not null)
insert into #ProductsTierPrice (Id)
select distinct Id 
from #ProductId P 
where P.HasTierPrices=1
			
if exists(select * from #ProductsTierPrice)
begin
	create table #TierPrice (ProductId int not null, Price decimal(18, 4) NOT NULL,Quantity int not null)
	insert into #TierPrice (ProductId,Price,Quantity)
	select T.ProductId,T.Price,T.Quantity
	from TierPrice T WITH (NOLOCK)
	where 
	--(T.StoreId=0 or T.StoreId=@StoreId) and
	 (T.CustomerRoleId is null or T.CustomerRoleId in (select CR.CustomerRoleId from #FilteredCustomerRoleIds CR))
			and T.ProductId in (select Id from #ProductsTierPrice)

	update #ProductId
	set Price=case
		when P.Price>TR.Price then TR.Price
		else P.Price
		end
	from #ProductId P,(select T.ProductId,min(T.Price) as Price
		from #TierPrice T 
			group by T.ProductId
				) as TR
	where P.Id=TR.ProductId
				
	drop table #TierPrice
end
drop table #ProductsTierPrice
--end TierPrice

--grouped product, get only one
if @grouped=1
begin
	delete from #ProductId
	where Id not in (
				SELECT TOP 1 WITH TIES P.Id
				FROM #ProductId P WITH (NOLOCK)
				ORDER BY ROW_NUMBER() OVER(PARTITION BY P.ParentGroupedProductId ORDER BY P.Price ASC))
end
--end TierPrice
END

GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fns_splitstring_group_to_table]') AND type in (N'F', N'TF'))
DROP FUNCTION [dbo].[fns_splitstring_group_to_table]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fns_splitstring_group_to_table]
(
	@string NVARCHAR(MAX),
	@delimiter CHAR(1),
	@delimitergroup CHAR(1)
)
RETURNS @output TABLE(
	data NVARCHAR(MAX),
	data2 NVARCHAR(MAX)
)
BEGIN
	--2>1,3>8,
	DECLARE @start INT, @end INT
	SELECT @start = 1, @end = CHARINDEX(@delimiter, @string)
	
	declare @group nvarchar(MAX)
	DECLARE @posmarker INT
	
	WHILE @start < LEN(@string) + 1 BEGIN
		IF @end = 0 
			SET @end = LEN(@string) + 1
		--2>1
		--145>748
		set @group=SUBSTRING(@string, @start, @end - @start)
		set @posmarker=CHARINDEX ('>',@group)
		if @posmarker<-1
		 set @posmarker=0
		
		INSERT INTO @output (data,data2) 
		VALUES(substring(@group,0,@posmarker),substring(@group,@posmarker+1,LEN(@group)))
		
		SET @start = @end + 1
		SET @end = CHARINDEX(@delimiter, @string, @start)
	END
	RETURN
END
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FNS_SpeedFilter_ProductLoadAllPaged]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[FNS_SpeedFilter_ProductLoadAllPaged]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[FNS_SpeedFilter_ProductLoadAllPaged]
(
	@CategoryId		int = 0,	
	@ManufacturerIds		nvarchar(MAX) = null,	--a list of manufacturer IDs (comma-separated list) for which a product should be shown
	@VendorIds			nvarchar(MAX) = null,	--a list of vendor IDs (comma-separated list) for which a product should be shown
	@StoreId			int = 0,
	@LanguageId			int = 0,
	@AllowedCustomerRoleIds	nvarchar(MAX) = null,	--a list of customer role IDs (comma-separated list) for which a product should be shown (if a subjet to ACL)
	@ShowProductsFromSubcategories bit=1, --Get Products From Subcategories
	@IgnoreDiscounts bit =1, --Ignore Discounts 
	@PageIndex			int = 0, 
	@PageSize			int = 2147483644,
	@FeaturedProducts	bit = null,	--0 featured only , 1 not featured only, null - load all products
	@Keywords			nvarchar(4000) = null,
	@SearchDescriptions bit = 0, --a value indicating whether to search by a specified "keyword" in product descriptions
	@UseFullTextSearch  bit = 0,
	@FullTextMode		int = 0, --0 - using CONTAINS with <prefix_term>, 5 - using CONTAINS and OR with <prefix_term>, 10 - using CONTAINS and AND with <prefix_term>
	@PriceMin			decimal(18, 4) = null,
	@PriceMax			decimal(18, 4) = null,
	@OrderBy			int = 0, --0 - position, 5 - Name: A to Z, 6 - Name: Z to A, 10 - Price: Low to High, 11 - Price: High to Low, 15 - creation date
	@showOnSaldo        nvarchar(4), -- all - all products, 'zal' Saldo, 'nozal' - nonSaldo
	@FilteredSpecs		nvarchar(MAX) = null,	--filter by specifucation attributes (comma-separated list). e.g. 1>47,1>58,3>45,3>4,3>56,1>58,1>47
	@FilteredAtrs		nvarchar(MAX) = null,	--filter by product attributes (comma-separated group list). e.g. 1>47,1>58,3>45,3>4,3>56,1>58,1>47
	@enablePriceRangeFilter bit=1,  -- enable Price Range Filter
	@enableSpecificationsFilter bit=1, -- enable Specifications Filter
	@enableAttributesFilter bit=1, -- enable Attributes Filter
	@enableManufacturersFilter bit=1, -- enable Manufacturers Filter
	@enableVendorsFilter bit=1, -- enable Vendors Filter
	@filtersConditionSpecifications bit=1, --  filters Condition for Specifications 1=AND, 0=OR
	@filtersConditionAttributes bit=1, --  filters Condition for Attributes 1=AND, 0=OR
	@filtersConditionBetweenBlocks bit=1 --  filters Condition between blocks 1=AND, 0=OR
)
AS
SET NOCOUNT ON
BEGIN

declare @FilterableSpecificationAttributeOptionIds nvarchar(MAX) = null --the specification attribute option identifiers applied to loaded products (all pages). returned as a comma separated list of identifiers
declare @FilterableProductAttributeOptionIds nvarchar(MAX) = null --the product attribute option identifiers applied to loaded products (all pages). returned as a comma separated list of identifiers
declare @FilterableManufacturerIds nvarchar(MAX) = null --the manufacturer identifiers applied to loaded products (all pages). returned as a comma separated list of identifiers
declare @FilterableVendorIds nvarchar(MAX) = null --the vendor identifiers applied to loaded products (all pages). returned as a comma separated list of identifiers

declare @TotalRecords int = null

--filter by customer role IDs (access control list)
SET @AllowedCustomerRoleIds = isnull(@AllowedCustomerRoleIds, '')	
CREATE TABLE #FilteredCustomerRoleIds
(
	CustomerRoleId int not null
)
INSERT INTO #FilteredCustomerRoleIds (CustomerRoleId)
SELECT CAST(data as int) FROM [nop_splitstring_to_table](@AllowedCustomerRoleIds, ',')	

--filter by manufacturer IDs (access control list)
SET @ManufacturerIds = isnull(@ManufacturerIds, '')	
CREATE TABLE #FilteredManufacturerIds
(
	Id int not null
)
INSERT INTO #FilteredManufacturerIds (Id)
SELECT CAST(data as int) FROM [nop_splitstring_to_table](@ManufacturerIds, ',')	

DECLARE @ManufacturerIdsCount int	
SET @ManufacturerIdsCount = (SELECT COUNT(1) FROM #FilteredManufacturerIds)

--filter by vendor IDs (access control list)
SET @VendorIds = isnull(@VendorIds, '')	
CREATE TABLE #FilteredVendorIds
(
	Id int not null
)
INSERT INTO #FilteredVendorIds (Id)
SELECT CAST(data as int) FROM [nop_splitstring_to_table](@VendorIds, ',')	
DECLARE @VendorIdsCount int	
SET @VendorIdsCount = (SELECT COUNT(1) FROM #FilteredVendorIds)

declare @sqlparce nvarchar(max)
--filter by specification
--1>47,1>58,3>45,3>4,3>56,1>58,1>47
SET @FilteredSpecs = isnull(@FilteredSpecs, '')	
CREATE TABLE #FilteredSpecs
(
	SpecificationAttributeId int not null,
	SpecificationAttributeOptionId int not null
)
set @sqlparce='INSERT INTO #FilteredSpecs (SpecificationAttributeId,SpecificationAttributeOptionId) SELECT CAST(data as int),CAST(data2 as int) FROM ['+SCHEMA_NAME()+'].[fns_splitstring_group_to_table]('''+@FilteredSpecs+''', '','', ''>'')'
exec (@sqlparce)
DECLARE @SpecAttributesCount int	
SET @SpecAttributesCount = (SELECT COUNT(1) FROM #FilteredSpecs)

--filter by product attributes
--1>47,1>58,3>45,3>4,3>56,1>58,1>47
SET @FilteredAtrs = isnull(@FilteredAtrs, '')	
CREATE TABLE #FilteredAtrs
(
	ProductAttributeId int not null,
	ProductAttributeOptionId int not null
)
set @sqlparce='INSERT INTO #FilteredAtrs (ProductAttributeId,ProductAttributeOptionId) SELECT CAST(data as int),CAST(data2 as int) FROM ['+SCHEMA_NAME()+'].[fns_splitstring_group_to_table]('''+@FilteredAtrs+''', '','', ''>'')'
exec (@sqlparce)
/*INSERT INTO #FilteredAtrs (ProductAttributeId,ProductAttributeOptionId)
SELECT CAST(data as int),CAST(data2 as int) FROM [fns_splitstring_group_to_table](@FilteredAtrs, ',', '>')*/

DECLARE @ProductAttributesCount int	
SET @ProductAttributesCount = (SELECT COUNT(1) FROM #FilteredAtrs)
------------------------------------------------------------------------------
	
if @LanguageId>0
begin	
	declare @countLanguage int
	set @countLanguage=0
	select @countLanguage=count(*)
	from Language L WITH (NOLOCK)
	where L.Published=1
			--and (L.LimitedToStores=0 or Exists(select Top 1 Id from StoreMapping SM   WITH (NOLOCK)
			--	where SM.EntityName = 'Language' and SM.StoreId=@storeId and SM.EntityId=L.Id))
	if @countLanguage=1
	begin
		set @LanguageId=0
	end
end

create table #UrlRecord	(ProductId int,SeName nvarchar(400))
create table #TierPriceId (ProductId int)
create table #ProductId (Id int, ParentGroupedProductId int, Price decimal(18,4), HasTierPrices bit )

	DECLARE
		@SearchKeywords bit,
		@sql nvarchar(max),
		@sql_orderby nvarchar(max)

	--filter by keywords
	SET @Keywords = isnull(@Keywords, '')
	SET @Keywords = rtrim(ltrim(@Keywords))
	SET @SearchKeywords=0
	IF ISNULL(@Keywords, '') != ''
	BEGIN
		SET @SearchKeywords = 1
		
		IF @UseFullTextSearch = 1
		BEGIN
			--remove wrong chars (' ")
			SET @Keywords = REPLACE(@Keywords, '''', '')
			SET @Keywords = REPLACE(@Keywords, '"', '')
			
			--full-text search
			IF @FullTextMode = 0 
			BEGIN
				--0 - using CONTAINS with <prefix_term>
				SET @Keywords = ' "' + @Keywords + '*" '
			END
			ELSE
			BEGIN
				--5 - using CONTAINS and OR with <prefix_term>
				--10 - using CONTAINS and AND with <prefix_term>

				--clean multiple spaces
				WHILE CHARINDEX('  ', @Keywords) > 0 
					SET @Keywords = REPLACE(@Keywords, '  ', ' ')

				DECLARE @concat_term nvarchar(100)				
				IF @FullTextMode = 5 --5 - using CONTAINS and OR with <prefix_term>
				BEGIN
					SET @concat_term = 'OR'
				END 
				IF @FullTextMode = 10 --10 - using CONTAINS and AND with <prefix_term>
				BEGIN
					SET @concat_term = 'AND'
				END

				--now let's build search string
				declare @fulltext_keywords nvarchar(4000)
				set @fulltext_keywords = N''
				declare @index int		
		
				set @index = CHARINDEX(' ', @Keywords, 0)

				-- if index = 0, then only one field was passed
				IF(@index = 0)
					set @fulltext_keywords = ' "' + @Keywords + '*" '
				ELSE
				BEGIN		
					DECLARE @first BIT
					SET  @first = 1			
					WHILE @index > 0
					BEGIN
						IF (@first = 0)
							SET @fulltext_keywords = @fulltext_keywords + ' ' + @concat_term + ' '
						ELSE
							SET @first = 0

						SET @fulltext_keywords = @fulltext_keywords + '"' + SUBSTRING(@Keywords, 1, @index - 1) + '*"'					
						SET @Keywords = SUBSTRING(@Keywords, @index + 1, LEN(@Keywords) - @index)						
						SET @index = CHARINDEX(' ', @Keywords, 0)
					end
					
					-- add the last field
					IF LEN(@fulltext_keywords) > 0
						SET @fulltext_keywords = @fulltext_keywords + ' ' + @concat_term + ' ' + '"' + SUBSTRING(@Keywords, 1, LEN(@Keywords)) + '*"'	
				END
				SET @Keywords = @fulltext_keywords
			END
		END
		ELSE
		BEGIN
			--usual search by PATINDEX
			SET @Keywords = '%' + @Keywords + '%'
		END
		--PRINT @Keywords
	END

	--filter by category IDs
	CREATE TABLE #FilteredCategoryIds
	(
		CategoryId int not null
	)

	if (@CategoryId>0)
	begin
		if @ShowProductsFromSubcategories=1
		begin
		--Category Child tree
		;with cte_tree_CategoryChild (Id,ParentCategoryId) as 
		(
			select C.Id,C.ParentCategoryId
			from Category C
			where C.Id=@CategoryId and C.Deleted=0 and C.Published=1
					and (C.SubjectToAcl = 0 OR EXISTS (
							SELECT 1 FROM #FilteredCustomerRoleIds [fcr]
								WHERE
									[fcr].CustomerRoleId IN (
										SELECT [acl].CustomerRoleId
										FROM [AclRecord] acl
										WHERE [acl].EntityId = C.Id AND [acl].EntityName = 'Category'
									)
								))
					--and (C.LimitedToStores=0 or Exists(select Id from StoreMapping SM   WITH (NOLOCK)
					--	where sm.EntityName = 'Category' and sm.StoreId=@storeId and sm.EntityId=C.Id))
			union all
			select C.Id,C.ParentCategoryId
			from cte_tree_CategoryChild T,Category C WITH (NOLOCK)
			where T.Id=C.ParentCategoryId and C.Deleted=0 and C.Published=1
					and (C.SubjectToAcl = 0 OR EXISTS (
							SELECT 1 FROM #FilteredCustomerRoleIds [fcr]
								WHERE
									[fcr].CustomerRoleId IN (
										SELECT [acl].CustomerRoleId
										FROM [AclRecord] acl
										WHERE [acl].EntityId = C.Id AND [acl].EntityName = 'Category'
									)
								))
					--and (C.LimitedToStores=0 or Exists(select Id from StoreMapping SM   WITH (NOLOCK)
					--	where sm.EntityName = 'Category' and sm.StoreId=@storeId and sm.EntityId=C.Id))
		)
		insert  into #FilteredCategoryIds (CategoryId)
		select Id
		from cte_tree_CategoryChild C
		end
		else
		begin
			insert  into #FilteredCategoryIds (CategoryId)
			values (@CategoryId)
		end
	end
		
	DECLARE @CategoryIdsCount int	
	SET @CategoryIdsCount = (SELECT COUNT(1) FROM #FilteredCategoryIds)


	--paging
	DECLARE @PageLowerBound int
	DECLARE @PageUpperBound int
	DECLARE @RowsToReturn int
	SET @RowsToReturn = @PageSize * (@PageIndex + 1)	
	SET @PageLowerBound = @PageSize * @PageIndex
	SET @PageUpperBound = @PageLowerBound + @PageSize + 1
	
	CREATE TABLE #DisplayOrderTmp 
	(
		[Id] int IDENTITY (1, 1) NOT NULL,
		[ProductId] int NOT NULL,
		[Allow] bit NULL
	)

	SET @sql = '
	INSERT INTO #DisplayOrderTmp ([ProductId])
	SELECT p.Id
	FROM
		Product p with (NOLOCK)'
	
	IF @CategoryIdsCount > 0
	BEGIN
		SET @sql = @sql + '
		LEFT JOIN Product_Category_Mapping pcm with (NOLOCK)
			ON p.Id = pcm.ProductId'
	END
	
	IF @ManufacturerIdsCount > 0
	BEGIN
		SET @sql = @sql + '
		LEFT JOIN Product_Manufacturer_Mapping pmm with (NOLOCK)
			ON p.Id = pmm.ProductId'
	END


	SET @sql = @sql + '
	WHERE
		p.Deleted = 0 AND p.Published = 1 '
	
	IF @SearchKeywords = 1
	BEGIN
		SET @sql = @sql + 'AND (
			'
		--product name 
		IF @UseFullTextSearch = 1
			SET @sql = @sql + '
			CONTAINS(p.[Name], @Keywords) '
		ELSE
			SET @sql = @sql + '
			PATINDEX(@Keywords, p.[Name]) > 0 '
		--SKU
		IF @UseFullTextSearch = 1
			SET @sql = @sql + '
			OR CONTAINS(p.[Sku], @Keywords) '
		ELSE
			SET @sql = @sql + '
			OR PATINDEX(@Keywords, p.[Sku]) > 0 '				

		if @LanguageId>0
		begin
		--localized product name
		SET @sql = @sql + ' 
			OR p.Id in (
				SELECT lp.EntityId
				FROM LocalizedProperty lp with (NOLOCK)
				WHERE
					lp.LocaleKeyGroup = N''Product''
					AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '
					AND lp.LocaleKey = N''Name'''
			IF @UseFullTextSearch = 1
				SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '
			ELSE
				SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '
			SET @sql = @sql + ')'	
		end
		IF @SearchDescriptions = 1
		BEGIN
			--product short description
			IF @UseFullTextSearch = 1
				SET @sql = @sql + '
					OR CONTAINS(p.[ShortDescription], @Keywords) '
			ELSE
				SET @sql = @sql + '
					OR PATINDEX(@Keywords, p.[ShortDescription]) > 0 '			

			--product full description
			IF @UseFullTextSearch = 1
				SET @sql = @sql + '
					OR CONTAINS(p.[FullDescription], @Keywords) '
			ELSE
				SET @sql = @sql + '
					OR PATINDEX(@Keywords, p.[FullDescription]) > 0 '

			if @LanguageId>0
			begin
				--localized product short description
				SET @sql = @sql + ' 
					OR p.Id in (
					SELECT lp.EntityId
					FROM LocalizedProperty lp with (NOLOCK)
					WHERE
						lp.LocaleKeyGroup = N''Product''
						AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '
						AND lp.LocaleKey = N''ShortDescription'''
				IF @UseFullTextSearch = 1
					SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '
				ELSE
					SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '
				SET @sql = @sql + ')'	

				--localized product full description
				SET @sql = @sql + ' 
					OR p.Id in (
						SELECT lp.EntityId
						FROM LocalizedProperty lp with (NOLOCK)
						WHERE
							lp.LocaleKeyGroup = N''Product''
							AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '
							AND lp.LocaleKey = N''FullDescription'''
					IF @UseFullTextSearch = 1
						SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '
					ELSE
						SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '
					SET @sql = @sql + ')'

			end
			--product tag
			SET @sql = @sql + ' 
				OR p.Id in (
					SELECT pptm.Product_Id
					FROM Product_ProductTag_Mapping pptm with(NOLOCK) INNER JOIN ProductTag pt with(NOLOCK) ON pt.Id = pptm.ProductTag_Id
					WHERE '
					IF @UseFullTextSearch = 1
						SET @sql = @sql + 'CONTAINS(pt.[Name], @Keywords) '
					ELSE
						SET @sql = @sql + 'PATINDEX(@Keywords, pt.[Name]) > 0 '
					SET @sql = @sql + ')'

			if @LanguageId>0
			begin
				--localized product tag
				SET @sql = @sql + ' 
					OR p.Id in (
						SELECT pptm.Product_Id
						FROM LocalizedProperty lp with (NOLOCK) INNER JOIN Product_ProductTag_Mapping pptm with(NOLOCK) ON lp.EntityId = pptm.ProductTag_Id
						WHERE
							lp.LocaleKeyGroup = N''ProductTag''
							AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '
							AND lp.LocaleKey = N''Name'''
				IF @UseFullTextSearch = 1
					SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '
				ELSE
					SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '
				SET @sql = @sql + ')'
			end
		END
		SET @sql = @sql + '
		)'
	END

	--filter by category
	IF @CategoryIdsCount > 0
	BEGIN
		SET @sql = @sql + '
		AND pcm.CategoryId IN (SELECT CategoryId FROM #FilteredCategoryIds)'
		
		IF @FeaturedProducts IS NOT NULL
		BEGIN
			SET @sql = @sql + '
		AND pcm.IsFeaturedProduct = ' + CAST(@FeaturedProducts AS nvarchar(max))
		END
	END
	
	--filter by manufacturer
	IF @ManufacturerIdsCount > 0
	BEGIN
		SET @sql = @sql + '
		AND pmm.ManufacturerId in (select Id from #FilteredManufacturerIds)'
		
		IF @FeaturedProducts IS NOT NULL
		BEGIN
			SET @sql = @sql + '
		AND pmm.IsFeaturedProduct = ' + CAST(@FeaturedProducts AS nvarchar(max))
		END
	END
	
	--filter by vendor
	IF @VendorIdsCount > 0
	BEGIN
		SET @sql = @sql + '
		AND p.VendorId in (select Id from #FilteredVendorIds)'
	END
	
	--filter by parent product identifer
	SET @sql = @sql + '
		AND p.VisibleIndividually = 1'

	
	--show hidden
	SET @sql = @sql + '
	AND (getutcdate() BETWEEN ISNULL(p.AvailableStartDateTimeUtc, ''1/1/1900'') and ISNULL(p.AvailableEndDateTimeUtc, ''1/1/2999''))'
	
	if @enablePriceRangeFilter=1
	Begin
		--min price
		IF @PriceMin > 0
		BEGIN
			SET @sql = @sql + '
				AND (p.Price >= ' + CAST(@PriceMin AS nvarchar(max)) + ')'
		END
		--max price
		IF @PriceMax > 0
		BEGIN
			SET @sql = @sql + '
				AND (p.Price <= ' + CAST(@PriceMax AS nvarchar(max)) + ')'
		END
	END
	
	IF @showOnSaldo='zal'
	begin
		SET @sql = @sql + '
		AND p.ProductTypeId=5 and p.StockQuantity>0'
	end
	IF @showOnSaldo='nozal'
	begin
		SET @sql = @sql + '
		AND p.ProductTypeId=5 and p.StockQuantity=0'
	end
		
	--show hidden and ACL
		SET @sql = @sql + '
		AND (p.SubjectToAcl = 0 OR EXISTS (
			SELECT 1 FROM #FilteredCustomerRoleIds [fcr]
			WHERE
				[fcr].CustomerRoleId IN (
					SELECT [acl].CustomerRoleId
					FROM [AclRecord] acl with (NOLOCK)
					WHERE [acl].EntityId = p.Id AND [acl].EntityName = ''Product''
				)
			))'
	
	--show hidden and filter by store
	--IF @StoreId > 0
	--BEGIN
	--	SET @sql = @sql + '
	--	AND (p.LimitedToStores = 0 OR EXISTS (
	--		SELECT 1 FROM [StoreMapping] sm with (NOLOCK)
	--		WHERE [sm].EntityId = p.Id AND [sm].EntityName = ''Product'' and [sm].StoreId=' + CAST(@StoreId AS nvarchar(max)) + '
	--		))'
	--END
	/*
	--filter by specs
	IF @SpecAttributesCount > 0
	BEGIN
		SET @sql = @sql + '
		AND NOT EXISTS (
			SELECT 1 FROM #FilteredSpecs [fs]
			WHERE
				[fs].SpecificationAttributeOptionId NOT IN (
					SELECT psam.SpecificationAttributeOptionId
					FROM Product_SpecificationAttribute_Mapping psam with (NOLOCK)
					WHERE psam.AllowFiltering = 1 AND psam.ProductId = p.Id
				)
			)'
	END
	*/	
	--sorting
	SET @sql_orderby = ''	
	IF @OrderBy = 5 /* Name: A to Z */
		SET @sql_orderby = ' p.[Name] ASC'
	ELSE IF @OrderBy = 6 /* Name: Z to A */
		SET @sql_orderby = ' p.[Name] DESC'
	ELSE IF @OrderBy = 10 /* Price: Low to High */
		SET @sql_orderby = ' p.[Price] ASC'
	ELSE IF @OrderBy = 11 /* Price: High to Low */
		SET @sql_orderby = ' p.[Price] DESC'
	ELSE IF @OrderBy = 15 /* creation date */
		SET @sql_orderby = ' p.[CreatedOnUtc] DESC'
	ELSE /* default sorting, 0 (position) */
	BEGIN
		IF @CategoryIdsCount = 0 and @ManufacturerIdsCount = 0 and @SearchKeywords=0
		begin
			SET @sql_orderby = ' p.DisplayOrder ASC'
		end
		--category position (display order)
		IF @CategoryIdsCount > 0 SET @sql_orderby = ' pcm.DisplayOrder ASC'
		
		--manufacturer position (display order)
		IF @ManufacturerIdsCount > 0
		BEGIN
			IF LEN(@sql_orderby) > 0 SET @sql_orderby = @sql_orderby + ', '
			SET @sql_orderby = @sql_orderby + ' pmm.DisplayOrder ASC'
		END
		/*
		--parent grouped product specified (sort associated products)
		IF @ParentGroupedProductId > 0
		BEGIN
			IF LEN(@sql_orderby) > 0 SET @sql_orderby = @sql_orderby + ', '
			SET @sql_orderby = @sql_orderby + ' p.[DisplayOrder] ASC'
		END
		*/
		--name
		IF LEN(@sql_orderby) > 0 SET @sql_orderby = @sql_orderby + ', '
		SET @sql_orderby = @sql_orderby + ' p.[Name] ASC'
	END
	
	SET @sql = @sql + '
	ORDER BY' + @sql_orderby
	
	--PRINT (@sql)
	EXEC sp_executesql @sql, N'@Keywords nvarchar(4000)', @Keywords

	DROP TABLE #FilteredCategoryIds

	CREATE TABLE #ProductFiltered (Id int, Allow bit)

	--filter by specs
	IF @SpecAttributesCount > 0
	BEGIN
		declare @SpecificationAttributeId int
		declare @SpecificationAttributeOptionId int

		DECLARE View_ProductSpecification INSENSITIVE CURSOR
		FOR SELECT distinct SpecificationAttributeId
		FROM #FilteredSpecs
		order by SpecificationAttributeId
		FOR READ ONLY

		OPEN View_ProductSpecification
		FETCH NEXT FROM View_ProductSpecification into @SpecificationAttributeId
		WHILE @@Fetch_Status=0
		begin
			delete from #ProductFiltered

			if @filtersConditionSpecifications=1
			BEGIN
				--AND
				declare @firstrunspec bit
				set @firstrunspec=1
				DECLARE View_ProductSpecificationOption INSENSITIVE CURSOR
				FOR SELECT distinct SpecificationAttributeOptionId
				FROM #FilteredSpecs
				where SpecificationAttributeId=@SpecificationAttributeId
				order by SpecificationAttributeOptionId
				FOR READ ONLY

				OPEN View_ProductSpecificationOption
				FETCH NEXT FROM View_ProductSpecificationOption into @SpecificationAttributeOptionId
				WHILE @@Fetch_Status=0
				begin
					if @firstrunspec=1
					begin
						set @firstrunspec=0
						insert into #ProductFiltered (Id,Allow)
						SELECT psam.ProductId,1
							FROM Product_SpecificationAttribute_Mapping psam with (NOLOCK)
							WHERE psam.AllowFiltering = 1 
								and psam.SpecificationAttributeOptionId=@SpecificationAttributeOptionId
					end
					else
					begin
						delete #ProductFiltered
						where  Id not in (
								SELECT psam.ProductId
								FROM Product_SpecificationAttribute_Mapping psam with (NOLOCK)
								WHERE psam.AllowFiltering = 1 
									and psam.SpecificationAttributeOptionId=@SpecificationAttributeOptionId)
					end
					FETCH NEXT FROM View_ProductSpecificationOption into @SpecificationAttributeOptionId
				end	
				DEALLOCATE View_ProductSpecificationOption
			END
			else
			BEGIN
				--OR
				insert into #ProductFiltered (Id,Allow)
				SELECT psam.ProductId,1
				FROM Product_SpecificationAttribute_Mapping psam with (NOLOCK)
				WHERE psam.AllowFiltering = 1 
					and psam.SpecificationAttributeOptionId in 
						(select SpecificationAttributeOptionId 
							from #FilteredSpecs 
						where SpecificationAttributeId=@SpecificationAttributeId)
			END
			--per block
			update D
			set Allow=case
					when @filtersConditionBetweenBlocks=1 and isnull(D.Allow,1)=1 and isnull(P.Allow,0)=1 then 1 --AND
					when @filtersConditionBetweenBlocks=0 and (isnull(D.Allow,0)=1 or isnull(P.Allow,0)=1) then 1 --OR
					else 0
				end
			from #DisplayOrderTmp D left join #ProductFiltered P on D.ProductId=P.Id

			FETCH NEXT FROM View_ProductSpecification into @SpecificationAttributeId
		end	
		DEALLOCATE View_ProductSpecification
	END
	--end filter by specs	

	--filter by product atributes
	IF @ProductAttributesCount > 0
	BEGIN
		declare @ProductAttributeId int
		declare @AttributeName NVARCHAR(MAX)
		create table #FilteredAtrsName (ProductAttributeId int, Name NVARCHAR(MAX))

		DECLARE View_ProductAttribute INSENSITIVE CURSOR
		FOR SELECT distinct ProductAttributeId
		FROM #FilteredAtrs
		order by ProductAttributeId
		FOR READ ONLY

		OPEN View_ProductAttribute
		FETCH NEXT FROM View_ProductAttribute into @ProductAttributeId
		WHILE @@Fetch_Status=0
		begin
			delete from #ProductFiltered
			delete from #FilteredAtrsName
			INSERT INTO #FilteredAtrsName (ProductAttributeId, Name)
			SELECT fa.ProductAttributeId,pva.Name
			FROM ProductAttributeValue pva WITH (NOLOCK),
				#FilteredAtrs fa
			where pva.Id=fa.ProductAttributeOptionId
				and fa.ProductAttributeId=@ProductAttributeId

			if @filtersConditionAttributes=1
			BEGIN
				--AND
				declare @firstrunatr bit
				set @firstrunatr=1
				--ProductAttributeId,ProductAttributeOptionId
				DECLARE View_ProductAttributeName INSENSITIVE CURSOR
				FOR SELECT distinct Name
				FROM #FilteredAtrsName
				FOR READ ONLY

				OPEN View_ProductAttributeName
				FETCH NEXT FROM View_ProductAttributeName into @AttributeName
				WHILE @@Fetch_Status=0
				begin
					if @firstrunatr=1
					begin
						set @firstrunatr=0
						insert into #ProductFiltered (Id,Allow)
						SELECT pam.ProductId,1
							FROM ProductAttributeValue pva WITH (NOLOCK),
								Product_ProductAttribute_Mapping pam WITH (NOLOCK)
							where pva.Name collate SQL_Latin1_General_CP1_CI_AS=@AttributeName collate SQL_Latin1_General_CP1_CI_AS
								and pva.ProductAttributeMappingId = pam.Id
								and pam.ProductAttributeId=@ProductAttributeId
					end
					else
					begin
						delete #ProductFiltered
						where  Id not in (
							SELECT pam.ProductId
								FROM ProductAttributeValue pva WITH (NOLOCK),
								Product_ProductAttribute_Mapping pam WITH (NOLOCK)
							where pva.Name collate SQL_Latin1_General_CP1_CI_AS=@AttributeName collate SQL_Latin1_General_CP1_CI_AS
								and pva.ProductAttributeMappingId = pam.Id
								and pam.ProductAttributeId=@ProductAttributeId)
					end
					FETCH NEXT FROM View_ProductAttributeName into @AttributeName
				end	
				DEALLOCATE View_ProductAttributeName
			END
			ELSE
			BEGIN
				--OR
				--ProductAttributeId,ProductAttributeOptionId
				insert into #ProductFiltered (Id,Allow)
				SELECT pam.ProductId,1
				FROM ProductAttributeValue pva WITH (NOLOCK),
					Product_ProductAttribute_Mapping pam WITH (NOLOCK),
					#FilteredAtrsName fa
				where pam.ProductAttributeId=@ProductAttributeId
					and pva.ProductAttributeMappingId = pam.Id
					and pva.Name collate SQL_Latin1_General_CP1_CI_AS=fa.Name collate SQL_Latin1_General_CP1_CI_AS
			END
			--per block
			update D
			set Allow=case
					when @filtersConditionBetweenBlocks=1 and isnull(D.Allow,1)=1 and isnull(P.Allow,0)=1 then 1 --AND
					when @filtersConditionBetweenBlocks=0 and (isnull(D.Allow,0)=1 or isnull(P.Allow,0)=1) then 1 --OR
					else 0
				end
			from #DisplayOrderTmp D left join #ProductFiltered P on D.ProductId=P.Id

			FETCH NEXT FROM View_ProductAttribute into @ProductAttributeId
		end	
		DEALLOCATE View_ProductAttribute

		drop table #FilteredAtrsName
	END
	--end filter by product atributes
	
	drop table #ProductFiltered

	IF @SpecAttributesCount > 0 or @ProductAttributesCount > 0
	begin
		delete from #DisplayOrderTmp where isnull(Allow,0)=0
	end

	CREATE TABLE #PageIndex 
	(
		[IndexId] int IDENTITY (1, 1) NOT NULL,
		[ProductId] int NOT NULL
	)
	INSERT INTO #PageIndex ([ProductId])
	SELECT ProductId
	FROM #DisplayOrderTmp
	GROUP BY ProductId
	ORDER BY min([Id])

	--total records
	SET @TotalRecords = @@rowcount
	
	DROP TABLE #DisplayOrderTmp

--SpecificationAttribute
if @enableSpecificationsFilter=1
begin
	SELECT @FilterableSpecificationAttributeOptionIds = COALESCE(@FilterableSpecificationAttributeOptionIds + ',' , '') + CAST(SpecificationAttributeOptionId as nvarchar(4000))
	from (
		select distinct PSM.SpecificationAttributeOptionId
		--select distinct PSM.SpecificationAttributeOptionId as Id
		from Product_SpecificationAttribute_Mapping PSM WITH (NOLOCK)
		WHERE PSM.ProductId in (select ProductId from #PageIndex)
			and PSM.AllowFiltering=1) D
end			
--End SpecificationAttribute

--ProductAttribute
if @enableAttributesFilter=1
begin

	SELECT @FilterableProductAttributeOptionIds = COALESCE(@FilterableProductAttributeOptionIds + ',' , '') + CAST(Id as nvarchar(4000))
	from (
		select distinct PVA.Id
			from ProductAttributeValue PVA WITH (NOLOCK),Product_ProductAttribute_Mapping PAM WITH (NOLOCK)
			where PVA.ProductAttributeMappingId=PAM.Id 
				and PAM.ProductId in (select ProductId from #PageIndex)
				and PAM.AttributeControlTypeId in (1,2,3,40,50)
		) D
end			
--End ProductAttribute

--Manufacturer
if @enableManufacturersFilter=1
begin
	SELECT @FilterableManufacturerIds = COALESCE(@FilterableManufacturerIds + ',' , '') + CAST(ManufacturerId as nvarchar(4000))
	from (
		select distinct PM.ManufacturerId
		from Product_Manufacturer_Mapping PM WITH (NOLOCK) 
		where PM.ProductId in (select ProductId from #PageIndex)
		) D
end			
--End Manufacturer		

--Vendor
if @enableVendorsFilter=1
begin
	if exists(select Id from Vendor)
	begin
		SELECT @FilterableVendorIds = COALESCE(@FilterableVendorIds + ',' , '') + CAST(VendorId as nvarchar(4000))
		from (
			select distinct P.VendorId
			from Product P WITH (NOLOCK) 
			where P.Id in (select ProductId from #PageIndex)
			) D
	end
	else
	begin
		set @FilterableVendorIds=null
	end
end		
--End Vendor

		
	--return products
	create table #Products (Id int Not null, ProductTypeId int,ParentGroupedProductId int, Name nvarchar(400), ShortDescription nvarchar(max), FullDescription nvarchar(max),
		Price decimal(18, 4) NOT NULL, HasTierPrices bit NOT NULL) 
	insert into #Products (Id,ProductTypeId,ParentGroupedProductId,Name,ShortDescription,FullDescription,
		Price,HasTierPrices)
	select TOP (@RowsToReturn) P.Id,P.ProductTypeId,P.ParentGroupedProductId,P.Name,P.ShortDescription,P.FullDescription,
		P.Price,P.HasTierPrices
	FROM
		#PageIndex [pi]
		INNER JOIN Product p with (NOLOCK) on p.Id = [pi].[ProductId]
	WHERE
		[pi].IndexId > @PageLowerBound AND 
		[pi].IndexId < @PageUpperBound
	ORDER BY
		[pi].IndexId
	
	DROP TABLE #PageIndex

		if exists(select * from #Products)
		begin
			if (@LanguageId>0)
			begin
				update #Products
				set Name=ltrim(rtrim(Substring(L.LocaleValue,1,400)))
				from #Products C, 
					LocalizedProperty L WITH (NOLOCK) 
				where C.Id=L.EntityId and L.LanguageId=@LanguageId and L.LocaleKeyGroup='Product' and L.LocaleKey='Name'
			
				update #Products
				set ShortDescription=L.LocaleValue
				from #Products C, 
					LocalizedProperty L WITH (NOLOCK) 
				where C.Id=L.EntityId and L.LanguageId=@LanguageId and L.LocaleKeyGroup='Product' and L.LocaleKey='ShortDescription'

				update #Products
				set FullDescription=L.LocaleValue
				from #Products C, 
					LocalizedProperty L WITH (NOLOCK) 
				where C.Id=L.EntityId and L.LanguageId=@LanguageId and L.LocaleKeyGroup='Product' and L.LocaleKey='FullDescription'
			end
			
			if @IgnoreDiscounts=1
			begin
				delete from #ProductId
				insert into #ProductId (Id, ParentGroupedProductId, Price, HasTierPrices)
				select Id,ParentGroupedProductId,Price,	HasTierPrices
				from #Products
				where ProductTypeId=5
				
				exec [FNS_SpeedFilter_ProductLoadAllPaged_GetMinimumPrice] @Storeid, 0
				
				update #Products
				set Price=P.Price
				from #Products F, #ProductId P
				where F.Id=P.Id
			end		
		
			--TierPrice
			insert into #TierPriceId (ProductId)
			select Id
			from #Products
			where HasTierPrices=1
						
			--UrlRecord SeName
			insert into #UrlRecord (ProductId,SeName)
			select U.EntityId as ProductId,ltrim(rtrim(Substring(U.Slug,1,400))) as SeName
			from UrlRecord U WITH (NOLOCK)
			where U.EntityId in (select Id from #Products) 
				and U.EntityName='Product' and U.LanguageId=0 and U.IsActive=1	
		end
		
	--#Products
	select F.Price, F.Name, F.ShortDescription, F.FullDescription, P.* 
	from Product P WITH(NOLOCK),#Products F
	where P.Id=F.Id
	
	--Group Product
	create table #GroupProducts (Id int Not null, ProductTypeId int,ParentGroupedProductId int, 
		Price decimal(18, 4) NOT NULL, HasTierPrices bit NOT NULL) 

	insert into #GroupProducts (Id,ProductTypeId,ParentGroupedProductId,
		Price,HasTierPrices)
	select P.Id,P.ProductTypeId,P.ParentGroupedProductId,
		P.Price,P.HasTierPrices
	from Product P With (NOLOCK)
	where P.ParentGroupedProductId in (select Id from #products PG where PG.ProductTypeId=10)
		and P.Published=1 and P.Deleted=0
		and (P.SubjectToAcl = 0 OR EXISTS (
							SELECT 1 FROM #FilteredCustomerRoleIds [fcr]
								WHERE
									[fcr].CustomerRoleId IN (
										SELECT [acl].CustomerRoleId
										FROM [AclRecord] acl
										WHERE [acl].EntityId = P.Id AND [acl].EntityName = 'Product'
									)
								))
		--and (P.LimitedToStores=0 or Exists(select Top 1 Id from StoreMapping SM   WITH (NOLOCK)
		--		where sm.EntityName = 'Product' and sm.StoreId=@storeId and sm.EntityId=P.Id))
	
	if @IgnoreDiscounts=1
	begin
		delete from #ProductId
		
		insert into #ProductId (Id, ParentGroupedProductId, Price, HasTierPrices)
		select P.Id,P.ParentGroupedProductId, Price, P.HasTierPrices
		from #GroupProducts P
		where P.ProductTypeId=5

		exec [FNS_SpeedFilter_ProductLoadAllPaged_GetMinimumPrice] @Storeid, 11
			
		delete from #GroupProducts
		where ProductTypeId=5 and Id not in (select Id from #ProductId)	
		
		update #GroupProducts
		set Price=P.Price
		from #GroupProducts F, #ProductId P
		where F.Id=P.Id
		
		delete from #ProductId
	end		

	select F.Price, P.* 
	from Product P WITH(NOLOCK),#GroupProducts F
	where P.Id=F.Id

	insert into #TierPriceId (ProductId)
	select Id
	from #GroupProducts
	where HasTierPrices=1
				
	drop table #GroupProducts
	drop table #Products

	select @TotalRecords as Total,
		@FilterableSpecificationAttributeOptionIds as FilterableSpecificationAttributeOptionIds,
		@FilterableProductAttributeOptionIds as FilterableProductAttributeOptionIds,
		@FilterableManufacturerIds as FilterableManufacturerIds,
		@FilterableVendorIds as FilterableVendorIds

--End Products

--UrlRecord
	
	if (@LanguageId>0)
	begin
		update #UrlRecord
		set SeName=ltrim(rtrim(Substring(US.Slug,1,400)))
		from #UrlRecord C, (select TOP 1 WITH TIES U.EntityId,U.Slug
			from UrlRecord U WITH (NOLOCK)
			where U.EntityId in (select ProductId from #UrlRecord) 
				and U.EntityName='Product' and U.LanguageId=@LanguageId and U.IsActive=1
				and ltrim(rtrim(U.Slug))!=''
			ORDER BY ROW_NUMBER() OVER(PARTITION BY U.EntityId ORDER BY U.Id DESC)) US
		where C.ProductId=US.EntityId
	end			
	
	select * from  #UrlRecord

--end UrlRecord

--TierPrice

	select T.* 
	from TierPrice T WITH (NOLOCK)
	where T.ProductId in (select ProductId from #TierPriceId)
		--and (T.StoreId=0 or T.StoreId=@StoreId) 
		and (T.CustomerRoleId is null or T.CustomerRoleId in (select CR.CustomerRoleId from #FilteredCustomerRoleIds CR))
	order by T.ProductId,T.Quantity 

--end TierPrice				

drop table #ProductId
drop table #TierPriceId
drop table #FilteredCustomerRoleIds
drop table #FilteredManufacturerIds
DROP TABLE #FilteredSpecs
DROP TABLE #FilteredAtrs
drop table #FilteredVendorIds
drop table #UrlRecord
	
END

GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FNS_SpeedFilter_Authorize_ById]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[FNS_SpeedFilter_Authorize_ById]
GO

/****** Object:  StoredProcedure [dbo].[FNS_SpeedFilter_Authorize_ById]    Script Date: 05/12/2014 14:19:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[FNS_SpeedFilter_Authorize_ById]
(
	@CategoryId		int = 0,	--a Category ID for which a product should be shown
	@ManufacturerId		int = 0,	--a manufacturer ID for which a product should be shown
	@VendorId			int = 0,	--a vendor ID for which a product should be shown
	@StoreId			int = 0,
	@AllowedCustomerRoleIds	nvarchar(MAX) = null	--a list of customer role IDs (comma-separated list) for which a product should be shown (if a subjet to ACL)
)
AS
SET NOCOUNT ON
BEGIN
declare @isAuthorize bit
set @isAuthorize=1
if @CategoryId=0 and @ManufacturerId=0 and @VendorId=0
begin
	set @isAuthorize=0
	select @isAuthorize as isAuthorize
	return
end

--filter by customer role IDs (access control list)
SET @AllowedCustomerRoleIds = isnull(@AllowedCustomerRoleIds, '')	
CREATE TABLE #FilteredCustomerRoleIds
(
	CustomerRoleId int not null
)
INSERT INTO #FilteredCustomerRoleIds (CustomerRoleId)
SELECT CAST(data as int) FROM [nop_splitstring_to_table](@AllowedCustomerRoleIds, ',')	

declare @Id int,@Published bit, @SubjectToAcl bit, @LimitedToStores bit
--Category
if @CategoryId>0
begin
	set @Id=0
	select @Id=C.Id,@Published=C.Published,@SubjectToAcl=C.SubjectToAcl,@LimitedToStores=C.LimitedToStores
	from Category C WITH (NOLOCK) 
	where C.Id=@CategoryId and C.Deleted=0 
	if (@Id=0)
		set @isAuthorize=0
		
	if @isAuthorize=1 and @Published=0 
	begin
		IF NOT EXISTS (SELECT 1 FROM #FilteredCustomerRoleIds [fcr]
						WHERE
						[fcr].CustomerRoleId IN (
						SELECT PM.CustomerRole_Id
							FROM PermissionRecord P WITH (NOLOCK),PermissionRecord_Role_Mapping PM WITH (NOLOCK)
							where P.SystemName='ManageCategories' and P.Id=PM.PermissionRecord_Id
						))
		begin
			set @isAuthorize=0
		end
	end		
	if @isAuthorize=1 and @SubjectToAcl=1
	begin
		if  NOT EXISTS (SELECT 1 FROM #FilteredCustomerRoleIds [fcr]
						WHERE
						[fcr].CustomerRoleId IN (
							SELECT [acl].CustomerRoleId
								FROM [AclRecord] acl
							WHERE [acl].EntityId = @Id AND [acl].EntityName = 'Category'))
		begin
			set @isAuthorize=0
		end
	end
		
	--if @isAuthorize=1 and @LimitedToStores=1
	--begin
	--	if NOT Exists(select Id from StoreMapping SM   WITH (NOLOCK)
	--				where SM.EntityName = 'Category' and SM.StoreId=@storeId and SM.EntityId=@Id)
	--	begin
	--		set @isAuthorize=0
	--	end
	--end

	select @isAuthorize as isAuthorize	
	return
end
--Manufacturer
if @ManufacturerId>0
begin
	set @Id=0
	select @Id=C.Id,@Published=C.Published,@SubjectToAcl=C.SubjectToAcl,@LimitedToStores=C.LimitedToStores
	from Manufacturer C WITH (NOLOCK) 
	where C.Id=@ManufacturerId and C.Deleted=0 
	if (@Id=0)
		set @isAuthorize=0
		
	if @isAuthorize=1 and @Published=0 
	begin
		IF NOT EXISTS (SELECT 1 FROM #FilteredCustomerRoleIds [fcr]
						WHERE
						[fcr].CustomerRoleId IN (
						SELECT PM.CustomerRole_Id
							FROM PermissionRecord P WITH (NOLOCK),PermissionRecord_Role_Mapping PM WITH (NOLOCK)
							where P.SystemName='ManageManufacturers' and P.Id=PM.PermissionRecord_Id
						))
		begin
			set @isAuthorize=0
		end
	end		
	if @isAuthorize=1 and @SubjectToAcl=1
	begin
		if  NOT EXISTS (SELECT 1 FROM #FilteredCustomerRoleIds [fcr]
						WHERE
						[fcr].CustomerRoleId IN (
							SELECT [acl].CustomerRoleId
								FROM [AclRecord] acl
							WHERE [acl].EntityId = @Id AND [acl].EntityName = 'Manufacturer'))
		begin
			set @isAuthorize=0
		end
	end
		
	if @isAuthorize=1 and @LimitedToStores=1
	begin
		if NOT Exists(select Id from StoreMapping SM   WITH (NOLOCK)
					where SM.EntityName = 'Manufacturer' and SM.StoreId=@storeId and SM.EntityId=@Id)
		begin
			set @isAuthorize=0
		end
	end

	select @isAuthorize as isAuthorize	
	return
end

--Vendor
if @VendorId>0
begin
	set @Id=0
	select @Id=C.Id
	from Vendor C WITH (NOLOCK) 
	where C.Id=@VendorId and C.Active=1 and C.Deleted=0 
	if (@Id=0)
		set @isAuthorize=0

	select @isAuthorize as isAuthorize	
	return
end

select @isAuthorize as isAuthorize	
END	
GO
--/************** Create function splitstring_to_table_with_Index if not exist ************/
--IF OBJECT_ID('dbo.splitstring_to_table_with_Index') IS NOT NULL

/****** Object: StoredProcedure  GenerateFilterUrlFromSeoUrl*****/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GenerateFilterUrlFromSeoUrl]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GenerateFilterUrlFromSeoUrl]
GO

/****** Object:  StoredProcedure [dbo].[GenerateFilterUrlFromSeoUrl] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GenerateFilterUrlFromSeoUrl] 
(@sFilter nvarchar(MAX)) 
AS 
BEGIN 
DECLARE @FinalUrl nvarchar(MAX)

CREATE TABLE #tblFilteredSpecification ( rowno int, specAttrNames nvarchar(MAX))

CREATE TABLE #tblSpecificationAttr_SpecificationAttrOption ( SpecId int,SpecName nvarchar(MAX), SpecOptions nvarchar(MAX),SpecOptionsName nvarchar(max))

INSERT INTO #tblFilteredSpecification(rowno,specAttrNames)
SELECT i.number,i.data
FROM [dbo].splitstring_to_table_with_Index(@sFilter,'_') AS i 

DECLARE @RowNo INT 
DECLARE @SaId INT 
DECLARE @SaName NVARCHAR(MAX) 
DECLARE @SaDisplayOrder int 
DECLARE specAttr_Cursor
CURSOR LOCAL FORWARD_ONLY
FOR

SELECT ROW_NUMBER() OVER(ORDER BY DisplayOrder DESC) AS rowno,Id,Name,DisplayOrder
FROM SpecificationAttribute
WHERE lower(name) IN ('production time','ink color','color','material')
ORDER BY DisplayOrder DESC 

OPEN specAttr_Cursor FETCH NEXT
FROM specAttr_Cursor INTO @RowNo,
                          @SaId,
                          @SaName,
                          @SaDisplayOrder WHILE @@FETCH_STATUS = 0 
BEGIN 
DECLARE @strAttrbutes nvarchar(MAX)
SET @strAttrbutes=(SELECT specAttrNames
   FROM #tblFilteredSpecification
   WHERE rowno=@RowNo) 

if(@strAttrbutes!='') 

BEGIN

CREATE TABLE #tblSpecificationAttrOption ( rowno int, specAttrOptionNames nvarchar(MAX))
INSERT INTO #tblSpecificationAttrOption(rowno,specAttrOptionNames)
SELECT i.number,i.data
FROM [dbo].splitstring_to_table_with_Index(@strAttrbutes,
                                           '~') AS i
INSERT INTO #tblSpecificationAttr_SpecificationAttrOption
SELECT DISTINCT @SaId,@SaName,
                STUFF(
                        (SELECT DISTINCT ',' + cast(t1.id AS nvarchar(MAX))
                         FROM SpecificationAttributeOption t1
                         INNER JOIN SpecificationAttributeOption t ON t1.id = t.id
                         WHERE t1.SpecificationAttributeId=@SaId
                           AND
                             (SELECT dbo.GetURLSlug(t1.Name, 'true')) IN
                             (SELECT specAttrOptionNames
                              FROM #tblSpecificationAttrOption)
                           FOR XML PATH ('')), 1,
                                               1,
                                               '')
					 ,STUFF(
                        (SELECT DISTINCT ' ' + cast(t1.Name AS nvarchar(MAX))
                         FROM SpecificationAttributeOption t1
                         INNER JOIN SpecificationAttributeOption t ON t1.id = t.id
                         WHERE t1.SpecificationAttributeId=@SaId
                           AND
                             (SELECT dbo.GetURLSlug(t1.Name, 'true')) IN
                             (SELECT specAttrOptionNames
                              FROM #tblSpecificationAttrOption)
                           FOR XML PATH ('')), 1,
                                               1,
                                               '')
DROP TABLE #tblSpecificationAttrOption END FETCH NEXT
FROM specAttr_Cursor INTO @RowNo,
                          @SaId,
                          @SaName,
                          @SaDisplayOrder END CLOSE specAttr_Cursor DEALLOCATE specAttr_Cursor
DECLARE @spcAttrResultUrl nvarchar(MAX)
DECLARE @spcAttrResultName nvarchar(MAX)
SET @spcAttrResultUrl=
  (SELECT DISTINCT STUFF(
                           (SELECT DISTINCT ';' +(CAST(t1.SpecId AS nvarchar(100))+'!'+t1.SpecOptions)
                            FROM #tblSpecificationAttr_SpecificationAttrOption t1
                            INNER JOIN #tblSpecificationAttr_SpecificationAttrOption t ON t1.SpecId = t.SpecId
                            FOR XML PATH ('')), 1, 1, ''))

SET @spcAttrResultName=
  (SELECT DISTINCT STUFF(
                           (SELECT DISTINCT ';' +(CAST(lower(t1.SpecName) AS nvarchar(100))+':'+t1.SpecOptionsName)FROM												#tblSpecificationAttr_SpecificationAttrOption t1
                            INNER JOIN #tblSpecificationAttr_SpecificationAttrOption t ON t1.SpecId = t.SpecId
                            FOR XML PATH ('')), 1, 1, ''))

if(isnull(@spcAttrResultUrl, '') !='') 
BEGIN
SET @FinalUrl='sFilters='+@spcAttrResultUrl 
END
DROP TABLE #tblSpecificationAttr_SpecificationAttrOption
DROP TABLE #tblFilteredSpecification
SELECT @FinalUrl as sFilter ,@spcAttrResultName as Name; ------------------------------------------
END
GO