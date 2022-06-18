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
		set Price = @price
		where ProductId = @productId and Quantity = @minQty; 
	end
	else
	begin
		-- insert Price
		insert into TierPrice
		values (@productId, 0, null, @minQty, @priceWithDiscount, null, null);
	end
	
	FETCH NEXT FROM CUR_TEST INTO @productId, @productName, @productNumber, @price, @discountAmount, @priceWithDiscount, @discountId, @minQty, @maxQty, @discountName;
END
CLOSE CUR_TEST
DEALLOCATE CUR_TEST
GO

