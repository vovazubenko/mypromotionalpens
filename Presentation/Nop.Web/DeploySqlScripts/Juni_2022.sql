-- Move from discount to tierList

declare 
	@productId int,
	@productName nvarchar(4000),
	@productNumber nvarchar(4000),
	@price [decimal] (18, 4),
	@discountAmount [decimal] (18, 4),
	@priceWithDiscount [decimal] (18, 4),
	@discountId int,
	@minQty int,
	@maxQty int,
	@discountName nvarchar(4000);
 
DECLARE CUR_TEST CURSOR FAST_FORWARD FOR
select 
	p.id as 'Product Id',
	p.Name as 'Product Name',
	p.Sku as 'Product Number',
	p.Price as 'Price',
	d.[DiscountAmount] as 'DiscountAmount',
	p.Price - d.[DiscountAmount] as 'Price with discount',
	pad.[Discount_Id] as 'Discount Id',
	d.[MinimumDiscountedQuantity] as 'Minimum Amount Quantity',
	d.[MaximumDiscountedQuantity] as 'Maximum Amount Quantity',
	d.[Name] as 'Discount Name'
from [dbo].[Product] p 
left join [dbo].[Discount_AppliedToProducts] pad on p.id = pad.[Product_Id]
left join [dbo].[Discount] d on d.[Id] = pad.[Discount_Id]
where p.Published = 1 and p.Deleted = 0 and pad.[Discount_Id] is not null
order by p.Name, d.[MinimumDiscountedQuantity];


OPEN CUR_TEST
FETCH NEXT FROM CUR_TEST INTO @productId, @productName, @productNumber, @price, @discountAmount, @priceWithDiscount, @discountId, @minQty, @maxQty, @discountName;


WHILE @@FETCH_STATUS = 0
BEGIN
	if exists(select 1 from TierPrice where ProductId = @productId and Quantity = @minQty)
	begin
		-- update Price
		update TierPrice
		set Price = @priceWithDiscount
		where ProductId = @productId and Quantity = @minQty; 
	end
	else
	begin
		-- insert Price
		insert into TierPrice
		values (@productId, 0, null, @minQty, @priceWithDiscount, null, null);
	end

    update [dbo].[Product]
    set [HasTierPrices] = 1
    where id = @productId;
	
	FETCH NEXT FROM CUR_TEST INTO @productId, @productName, @productNumber, @price, @discountAmount, @priceWithDiscount, @discountId, @minQty, @maxQty, @discountName;
END
CLOSE CUR_TEST
DEALLOCATE CUR_TEST
GO


-- SAV-65(d)
delete from [dbo].[Discount_AppliedToProducts];
delete from [dbo].[Discount_AppliedToCategories];
delete from [dbo].[Discount];
    

-- ADMIN DATA
insert into [dbo].[LocaleStringResource]
values (1, 'Admin.Catalog.Products.Fields.TierPriceRange', 'Price range'),
	   (1, 'Admin.Catalog.Products.Fields.MinQTY', 'Minimum quantity');


-- SAV-65(a)
alter table [dbo].[TierPrice]
add Cost [decimal](18, 4) NOT NULL default(0);

insert into [dbo].[LocaleStringResource]
values 
    (1, 'Admin.Catalog.Products.TierPrices.Fields.Cost', 'Cost'),
    (1, 'Admin.Catalog.Products.TierPrices.Fields.Cost.Hint', 'Specify the cost.');


-- SAV-65(b)
alter table [dbo].[Product]
add SetupCost [decimal](18, 4) NOT NULL default(0);

insert into [dbo].[LocaleStringResource]
values (1, 'Admin.Catalog.Products.Fields.SetUpCost', 'Setup Cost');


-- SAV-65(m)
insert into [dbo].[LocaleStringResource]
values
    (1, 'Admin.Common.ExportToExcel.All.Pricing', 'Export Pricing to Excel (all found)'),
    (1, 'Admin.Common.ExportToExcel.Selected.Pricing', 'Export Pricing to Excel (selected)'),
    (1, 'Admin.Common.Import.Products', 'Import Products'),
    (1, 'Admin.Common.Import.Pricing', 'Import Pricing'),
    (1, 'Admin.Catalog.Pricing.Imported', 'Tier prices have been imported successfully.'),
    (1, 'ActivityLog.ImportTierPrices', '{0} tier prices were imported'),
    (1, 'ActivityLog.ImportTierPricesError', 'Error by importing tier prices: could not find product with id {0} / SKU {1} / Name {2}.');

alter table [dbo].[TierPrice]
add MSRP [decimal](18, 4) NOT NULL default(0);

insert into [dbo].[ActivityLogType]
values 
    ('ImportTierPrices', 'Tier prices were imported', 1),
    ('ImportTierPricesError', 'Error by importing tier prices', 1);


-- JULY CHANGES HERE!!!
-- SAV-65(3)
insert into [dbo].[LocaleStringResource]
values
    (1, 'Admin.Catalog.Products.TierPrices.Fields.MSRP', 'MSRP'),
    (1, 'Admin.Catalog.Products.TierPrices.Fields.Discount', 'Discount'),
    (1, 'Admin.Catalog.Products.TierPrices.Fields.MSRP.Hint', 'Specify the MSRP.');

