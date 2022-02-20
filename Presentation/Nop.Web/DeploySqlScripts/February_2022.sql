

-- SAV-40
insert into [dbo].[LocaleStringResource]
values 
	(1, 'MenuItem.ViewedProducts', 'Viewed Products'),
	(1, 'MenuItem.CompareProducts', 'Compare Products');


update [dbo].[LocaleStringResource]
set [ResourceValue] = 'Most Popular'
where [ResourceName] = 'Enums.Nop.Core.Domain.Catalog.ProductSortingEnum.Position';


update [dbo].[LocaleStringResource]
set [ResourceValue] = 'New Products'
where [ResourceName] = 'Enums.Nop.Core.Domain.Catalog.ProductSortingEnum.CreatedOn';




-- SAV-37
update [dbo].[Category]
set [CategoryTemplateId] = 0
where [Name] = 'All Products';


declare @allProductCategory int = (select [Id] from [dbo].[Category] where [Name] = 'All Products');

DECLARE @MyCursor CURSOR;
DECLARE @MyField int;
BEGIN
    SET @MyCursor = CURSOR FOR
    select Id from Product;    

    OPEN @MyCursor 
    FETCH NEXT FROM @MyCursor 
    INTO @MyField

    WHILE @@FETCH_STATUS = 0
    BEGIN

	  insert into [dbo].[Product_Category_Mapping]
	  values (@MyField, @allProductCategory, 0, 0);

      FETCH NEXT FROM @MyCursor 
      INTO @MyField 
    END; 

    CLOSE @MyCursor ;
    DEALLOCATE @MyCursor;
END;


-- SAV-38

IF NOT EXISTS (
    select * from sysobjects where name='categories_redirect_from' and xtype='U'
) CREATE TABLE categories_redirect_from (
    [Redirect_From_URL] NVARCHAR(71),
    [Redirect_To_URL] NVARCHAR(57)
);

INSERT INTO categories_redirect_from VALUES
    (N'mypromotionalpens.com/Custom_Brand',N'mypromotionalpens.com/Brand'),
    (N'mypromotionalpens.com/Custom_Price',N'mypromotionalpens.com/Price'),
    (N'mypromotionalpens.com/Custom_Featured-Products',N'mypromotionalpens.com/Featured-Products'),
    (N'mypromotionalpens.com/Custom_Plastic-Pens',N'mypromotionalpens.com/Plastic-Pens'),
    (N'mypromotionalpens.com/Custom_Eco-Pens',N'mypromotionalpens.com/Eco-Pens'),
    (N'mypromotionalpens.com/Custom_1-Day-Rush-Pens',N'mypromotionalpens.com/1-Day-Rush-Pens'),
    (N'mypromotionalpens.com/Custom_Low-Minimum-Pens',N'mypromotionalpens.com/Low-Minimum-Pens'),
    (N'mypromotionalpens.com/Custom_Blue-Ink-Pens',N'mypromotionalpens.com/Blue-Ink-Pens'),
    (N'mypromotionalpens.com/Custom_Grip-Pens',N'mypromotionalpens.com/Grip-Pens'),
    (N'mypromotionalpens.com/Custom_Multi-Color-Imprint-Pens',N'mypromotionalpens.com/Multi-Color-Imprint-Pens'),
    (N'mypromotionalpens.com/Custom_Brand-Name-Pens',N'mypromotionalpens.com/Brand-Name-Pens'),
    (N'mypromotionalpens.com/Custom_Stylus-Pens',N'mypromotionalpens.com/Stylus-Pens'),
    (N'mypromotionalpens.com/Custom_All-Pens',N'mypromotionalpens.com/All-Pens'),
    (N'mypromotionalpens.com/Custom_Free-Setup-Pens',N'mypromotionalpens.com/Free-Setup-Pens'),
    (N'mypromotionalpens.com/Custom_No-Minimum-Pens',N'mypromotionalpens.com/No-Minimum-Pens'),
    (N'mypromotionalpens.com/Custom_Gel-Pens',N'mypromotionalpens.com/Gel-Pens'),
    (N'mypromotionalpens.com/Custom_Gift-Card',N'mypromotionalpens.com/Gift-Card'),
    (N'mypromotionalpens.com/Custom_24-Hour-Rush',N'mypromotionalpens.com/24-Hour-Rush'),
    (N'mypromotionalpens.com/Custom_Specials',N'mypromotionalpens.com/Specials'),
    (N'mypromotionalpens.com/Custom_Popular-Products',N'mypromotionalpens.com/Popular-Products'),
    (N'mypromotionalpens.com/Custom_All-Products',N'mypromotionalpens.com/All-Products'),
    (N'mypromotionalpens.com/Custom_COVID-19-Novel-Coronavirus',N'mypromotionalpens.com/COVID-19-Novel-Coronavirus'),
    (N'mypromotionalpens.com/Custom_Apparel',N'mypromotionalpens.com/Apparel'),
    (N'mypromotionalpens.com/Custom_Bracelets',N'mypromotionalpens.com/Bracelets'),
    (N'mypromotionalpens.com/Custom_Wristbands',N'mypromotionalpens.com/Wristbands'),
    (N'mypromotionalpens.com/Custom_Sports-Type-Wristbands',N'mypromotionalpens.com/Sports-Type-Wristbands'),
    (N'mypromotionalpens.com/Custom_Shirts',N'mypromotionalpens.com/Shirts'),
    (N'mypromotionalpens.com/Custom_Performance-Apparel',N'mypromotionalpens.com/Performance-Apparel'),
    (N'mypromotionalpens.com/Custom_Sweat-Shirts',N'mypromotionalpens.com/Sweat-Shirts'),
    (N'mypromotionalpens.com/Custom_Childrens-Sweat-Shirts',N'mypromotionalpens.com/Childrens-Sweat-Shirts'),
    (N'mypromotionalpens.com/Custom_Mens-Sweat-Shirts',N'mypromotionalpens.com/Mens-Sweat-Shirts'),
    (N'mypromotionalpens.com/Custom_Unisex-Sweat-Shirts',N'mypromotionalpens.com/Unisex-Sweat-Shirts'),
    (N'mypromotionalpens.com/Custom_Womens',N'mypromotionalpens.com/Womens'),
    (N'mypromotionalpens.com/Custom_T-Shirts',N'mypromotionalpens.com/T-Shirts'),
    (N'mypromotionalpens.com/Custom_Childrens-T-Shirts',N'mypromotionalpens.com/Childrens-T-Shirts'),
    (N'mypromotionalpens.com/Custom_Mens-T-Shirts',N'mypromotionalpens.com/Mens-T-Shirts'),
    (N'mypromotionalpens.com/Custom_Unisex-T-Shirts',N'mypromotionalpens.com/Unisex-T-Shirts'),
    (N'mypromotionalpens.com/Custom_Womens-T-Shirts',N'mypromotionalpens.com/Womens-T-Shirts'),
    (N'mypromotionalpens.com/Custom_Childrens-Polo-Shirts',N'mypromotionalpens.com/Childrens-Polo-Shirts'),
    (N'mypromotionalpens.com/Custom_Dress-Shirts',N'mypromotionalpens.com/Dress-Shirts'),
    (N'mypromotionalpens.com/Custom_Mens-Polo-Shirts',N'mypromotionalpens.com/Mens-Polo-Shirts'),
    (N'mypromotionalpens.com/Custom_Mens-Shirts',N'mypromotionalpens.com/Mens-Shirts'),
    (N'mypromotionalpens.com/Custom_Tank-Tops-Muscle-Shirts',N'mypromotionalpens.com/Tank-Tops-Muscle-Shirts'),
    (N'mypromotionalpens.com/Custom_Turtleneck-Shirts',N'mypromotionalpens.com/Turtleneck-Shirts'),
    (N'mypromotionalpens.com/Custom_Turtleneck-Sweaters',N'mypromotionalpens.com/Turtleneck-Sweaters'),
    (N'mypromotionalpens.com/Custom_Unisex-Polo-Shirts',N'mypromotionalpens.com/Unisex-Polo-Shirts'),
    (N'mypromotionalpens.com/Custom_Unisex-Shirts',N'mypromotionalpens.com/Unisex-Shirts'),
    (N'mypromotionalpens.com/Custom_Womens-Polo-Shirts',N'mypromotionalpens.com/Womens-Polo-Shirts'),
    (N'mypromotionalpens.com/Custom_Silly-Bandz',N'mypromotionalpens.com/Silly-Bandz'),
    (N'mypromotionalpens.com/Custom_Hair-Ties-Bands',N'mypromotionalpens.com/Hair-Ties-Bands'),
    (N'mypromotionalpens.com/Custom_Caps-Hats',N'mypromotionalpens.com/Caps-Hats'),
    (N'mypromotionalpens.com/Custom_Baseball-Caps',N'mypromotionalpens.com/Baseball-Caps'),
    (N'mypromotionalpens.com/Custom_Five-Panel-Caps',N'mypromotionalpens.com/Five-Panel-Caps'),
    (N'mypromotionalpens.com/Custom_Sandwich-Bill-Caps',N'mypromotionalpens.com/Sandwich-Bill-Caps'),
    (N'mypromotionalpens.com/Custom_Six-Panel-Caps',N'mypromotionalpens.com/Six-Panel-Caps'),
    (N'mypromotionalpens.com/Custom_Beanie',N'mypromotionalpens.com/Beanie'),
    (N'mypromotionalpens.com/Custom_Aprons',N'mypromotionalpens.com/Aprons'),
    (N'mypromotionalpens.com/Custom_Bib-Style-Aprons',N'mypromotionalpens.com/Bib-Style-Aprons'),
    (N'mypromotionalpens.com/Custom_Arm-Bands',N'mypromotionalpens.com/Arm-Bands'),
    (N'mypromotionalpens.com/Custom_Bandannas',N'mypromotionalpens.com/Bandannas'),
    (N'mypromotionalpens.com/Custom_Gloves',N'mypromotionalpens.com/Gloves'),
    (N'mypromotionalpens.com/Custom_Headbands',N'mypromotionalpens.com/Headbands'),
    (N'mypromotionalpens.com/Custom_Jackets',N'mypromotionalpens.com/Jackets'),
    (N'mypromotionalpens.com/Custom_Jacket-Pullovers',N'mypromotionalpens.com/Jacket-Pullovers'),
    (N'mypromotionalpens.com/Custom_Varsity-Letterman-Jackets',N'mypromotionalpens.com/Varsity-Letterman-Jackets'),
    (N'mypromotionalpens.com/Custom_Waterproof-Water-Repellent-Jackets',N'mypromotionalpens.com/Waterproof-Water-Repellent-Jackets'),
    (N'mypromotionalpens.com/Custom_Windbreakers',N'mypromotionalpens.com/Windbreakers'),
    (N'mypromotionalpens.com/Custom_Scarves',N'mypromotionalpens.com/Scarves'),
    (N'mypromotionalpens.com/Custom_Slippers',N'mypromotionalpens.com/Slippers'),
    (N'mypromotionalpens.com/Custom_Uniforms',N'mypromotionalpens.com/Uniforms'),
    (N'mypromotionalpens.com/Custom_Medical-Uniforms',N'mypromotionalpens.com/Medical-Uniforms'),
    (N'mypromotionalpens.com/Custom_Scrubs',N'mypromotionalpens.com/Scrubs'),
    (N'mypromotionalpens.com/Custom_Vests',N'mypromotionalpens.com/Vests'),
    (N'mypromotionalpens.com/Custom_Safety-Vests',N'mypromotionalpens.com/Safety-Vests'),
    (N'mypromotionalpens.com/Custom_Ponchos',N'mypromotionalpens.com/Ponchos'),
    (N'mypromotionalpens.com/Custom_Pullover',N'mypromotionalpens.com/Pullover'),
    (N'mypromotionalpens.com/Custom_Metal-Pens',N'mypromotionalpens.com/Metal-Pens'),
    (N'mypromotionalpens.com/Custom_Bags',N'mypromotionalpens.com/Bags'),
    (N'mypromotionalpens.com/Custom_Coolers-Lunch-Bags',N'mypromotionalpens.com/Coolers-Lunch-Bags'),
    (N'mypromotionalpens.com/Custom_Tote-Bags',N'mypromotionalpens.com/Tote-Bags'),
    (N'mypromotionalpens.com/Custom_Backpacks',N'mypromotionalpens.com/Backpacks'),
    (N'mypromotionalpens.com/Custom_Duffel-Bags',N'mypromotionalpens.com/Duffel-Bags'),
    (N'mypromotionalpens.com/Custom_Fanny-Packs',N'mypromotionalpens.com/Fanny-Packs'),
    (N'mypromotionalpens.com/Custom_Messenger,-Briefs-Laptop-Bags',N'mypromotionalpens.com/Messenger,-Briefs-Laptop-Bags'),
    (N'mypromotionalpens.com/Custom_Travel-Storage-Bags',N'mypromotionalpens.com/Travel-Storage-Bags'),
    (N'mypromotionalpens.com/Custom_Gift-Bags',N'mypromotionalpens.com/Gift-Bags'),
    (N'mypromotionalpens.com/Custom_Pet-Carrier-Bags',N'mypromotionalpens.com/Pet-Carrier-Bags'),
    (N'mypromotionalpens.com/Custom_Drawstring-Bags',N'mypromotionalpens.com/Drawstring-Bags'),
    (N'mypromotionalpens.com/Custom_Cosmetic-Bags',N'mypromotionalpens.com/Cosmetic-Bags'),
    (N'mypromotionalpens.com/Custom_Shopping-Bags',N'mypromotionalpens.com/Shopping-Bags'),
    (N'mypromotionalpens.com/Custom_Bottle-Bags',N'mypromotionalpens.com/Bottle-Bags'),
    (N'mypromotionalpens.com/Custom_Drinkware',N'mypromotionalpens.com/Drinkware'),
    (N'mypromotionalpens.com/Custom_Water-Bottles',N'mypromotionalpens.com/Water-Bottles'),
    (N'mypromotionalpens.com/Custom_Bike-Sport-Bottles',N'mypromotionalpens.com/Bike-Sport-Bottles'),
    (N'mypromotionalpens.com/Custom_Bottled-Water',N'mypromotionalpens.com/Bottled-Water'),
    (N'mypromotionalpens.com/Custom_Metal-Bottles',N'mypromotionalpens.com/Metal-Bottles'),
    (N'mypromotionalpens.com/Custom_Plastic-Bottles',N'mypromotionalpens.com/Plastic-Bottles'),
    (N'mypromotionalpens.com/Custom_Glass-Water-Bottles',N'mypromotionalpens.com/Glass-Water-Bottles'),
    (N'mypromotionalpens.com/Custom_Coffee-Wraps',N'mypromotionalpens.com/Coffee-Wraps'),
    (N'mypromotionalpens.com/Custom_Glass-Stemware',N'mypromotionalpens.com/Glass-Stemware'),
    (N'mypromotionalpens.com/Custom_Glasses',N'mypromotionalpens.com/Glasses'),
    (N'mypromotionalpens.com/Custom_Beer-Jars',N'mypromotionalpens.com/Beer-Jars'),
    (N'mypromotionalpens.com/Custom_Champagne',N'mypromotionalpens.com/Champagne'),
    (N'mypromotionalpens.com/Custom_Wine-Glasses',N'mypromotionalpens.com/Wine-Glasses'),
    (N'mypromotionalpens.com/Custom_Martini-Glasses',N'mypromotionalpens.com/Martini-Glasses'),
    (N'mypromotionalpens.com/Custom_Old-Fashioned-Glasses',N'mypromotionalpens.com/Old-Fashioned-Glasses'),
    (N'mypromotionalpens.com/Custom_Pilsner-Glasses',N'mypromotionalpens.com/Pilsner-Glasses'),
    (N'mypromotionalpens.com/Custom_Whiskey-Glasses',N'mypromotionalpens.com/Whiskey-Glasses'),
    (N'mypromotionalpens.com/Custom_Tumblers',N'mypromotionalpens.com/Tumblers'),
    (N'mypromotionalpens.com/Custom_Cups',N'mypromotionalpens.com/Cups'),
    (N'mypromotionalpens.com/Custom_Plastic-Stadium-Cups',N'mypromotionalpens.com/Plastic-Stadium-Cups'),
    (N'mypromotionalpens.com/Custom_Metal-Cups',N'mypromotionalpens.com/Metal-Cups'),
    (N'mypromotionalpens.com/Custom_Shot-Glasses',N'mypromotionalpens.com/Shot-Glasses'),
    (N'mypromotionalpens.com/Custom_Mugs',N'mypromotionalpens.com/Mugs'),
    (N'mypromotionalpens.com/Custom_Coffee-Mugs',N'mypromotionalpens.com/Coffee-Mugs'),
    (N'mypromotionalpens.com/Custom_Travel-Mugs',N'mypromotionalpens.com/Travel-Mugs'),
    (N'mypromotionalpens.com/Custom_Bottle-Openers',N'mypromotionalpens.com/Bottle-Openers'),
    (N'mypromotionalpens.com/Custom_Coasters',N'mypromotionalpens.com/Coasters'),
    (N'mypromotionalpens.com/Custom_Flasks,-Growlers-Kegs',N'mypromotionalpens.com/Flasks,-Growlers-Kegs'),
    (N'mypromotionalpens.com/Custom_Thermoses',N'mypromotionalpens.com/Thermoses'),
    (N'mypromotionalpens.com/Custom_Steins',N'mypromotionalpens.com/Steins'),
    (N'mypromotionalpens.com/Custom_Ceramic-Mugs-Steins',N'mypromotionalpens.com/Ceramic-Mugs-Steins'),
    (N'mypromotionalpens.com/Custom_Glass-Mugs-Steins',N'mypromotionalpens.com/Glass-Mugs-Steins'),
    (N'mypromotionalpens.com/Custom_Metal-Mugs-Steins',N'mypromotionalpens.com/Metal-Mugs-Steins'),
    (N'mypromotionalpens.com/Custom_Plastic-Mugs-Steins',N'mypromotionalpens.com/Plastic-Mugs-Steins'),
    (N'mypromotionalpens.com/Custom_Soup-Type-Mugs-Steins',N'mypromotionalpens.com/Soup-Type-Mugs-Steins'),
    (N'mypromotionalpens.com/Custom_Stoneware-Mugs-Steins',N'mypromotionalpens.com/Stoneware-Mugs-Steins'),
    (N'mypromotionalpens.com/Custom_Beverage-Holders',N'mypromotionalpens.com/Beverage-Holders'),
    (N'mypromotionalpens.com/Custom_Koozies',N'mypromotionalpens.com/Koozies'),
    (N'mypromotionalpens.com/Custom_Beverage-Sleeves',N'mypromotionalpens.com/Beverage-Sleeves'),
    (N'mypromotionalpens.com/Custom_Jars',N'mypromotionalpens.com/Jars'),
    (N'mypromotionalpens.com/Custom_Shakers',N'mypromotionalpens.com/Shakers'),
    (N'mypromotionalpens.com/Custom_Food-Candy',N'mypromotionalpens.com/Food-Candy'),
    (N'mypromotionalpens.com/Custom_Utensils',N'mypromotionalpens.com/Utensils'),
    (N'mypromotionalpens.com/Custom_Candy',N'mypromotionalpens.com/Candy'),
    (N'mypromotionalpens.com/Custom_Jelly-Beans',N'mypromotionalpens.com/Jelly-Beans'),
    (N'mypromotionalpens.com/Custom_Mints',N'mypromotionalpens.com/Mints'),
    (N'mypromotionalpens.com/Custom_Chocolate',N'mypromotionalpens.com/Chocolate'),
    (N'mypromotionalpens.com/Custom_Popcorn',N'mypromotionalpens.com/Popcorn'),
    (N'mypromotionalpens.com/Custom_Tea',N'mypromotionalpens.com/Tea'),
    (N'mypromotionalpens.com/Custom_Nuts',N'mypromotionalpens.com/Nuts'),
    (N'mypromotionalpens.com/Custom_Cocoa',N'mypromotionalpens.com/Cocoa'),
    (N'mypromotionalpens.com/Custom_Office-Awards',N'mypromotionalpens.com/Office-Awards'),
    (N'mypromotionalpens.com/Custom_Desktop-Items',N'mypromotionalpens.com/Desktop-Items'),
    (N'mypromotionalpens.com/Custom_Business-Card-Holders',N'mypromotionalpens.com/Business-Card-Holders'),
    (N'mypromotionalpens.com/Custom_Desk-Clocks',N'mypromotionalpens.com/Desk-Clocks'),
    (N'mypromotionalpens.com/Custom_Desk-Pen-Stands',N'mypromotionalpens.com/Desk-Pen-Stands'),
    (N'mypromotionalpens.com/Custom_Pen-Pencil-Holders',N'mypromotionalpens.com/Pen-Pencil-Holders'),
    (N'mypromotionalpens.com/Custom_Planners',N'mypromotionalpens.com/Planners'),
    (N'mypromotionalpens.com/Custom_Memo-Holders',N'mypromotionalpens.com/Memo-Holders'),
    (N'mypromotionalpens.com/Custom_Tape',N'mypromotionalpens.com/Tape'),
    (N'mypromotionalpens.com/Custom_Tape-Dispensers',N'mypromotionalpens.com/Tape-Dispensers'),
    (N'mypromotionalpens.com/Custom_Desk-Accessories---Decorative',N'mypromotionalpens.com/Desk-Accessories---Decorative'),
    (N'mypromotionalpens.com/Custom_Desk-Caddy-Trays',N'mypromotionalpens.com/Desk-Caddy-Trays'),
    (N'mypromotionalpens.com/Custom_Desk-Sets',N'mypromotionalpens.com/Desk-Sets'),
    (N'mypromotionalpens.com/Custom_Lapel-Pins',N'mypromotionalpens.com/Lapel-Pins'),
    (N'mypromotionalpens.com/Custom_Mouse-Pads',N'mypromotionalpens.com/Mouse-Pads'),
    (N'mypromotionalpens.com/Custom_Buttons',N'mypromotionalpens.com/Buttons'),
    (N'mypromotionalpens.com/Custom_Awards,-Plaques-Trophies',N'mypromotionalpens.com/Awards,-Plaques-Trophies'),
    (N'mypromotionalpens.com/Custom_Letter-Openers',N'mypromotionalpens.com/Letter-Openers'),
    (N'mypromotionalpens.com/Custom_Stress-Relievers',N'mypromotionalpens.com/Stress-Relievers'),
    (N'mypromotionalpens.com/Custom_Stress-Balls',N'mypromotionalpens.com/Stress-Balls'),
    (N'mypromotionalpens.com/Custom_Stationary',N'mypromotionalpens.com/Stationary'),
    (N'mypromotionalpens.com/Custom_Stickers',N'mypromotionalpens.com/Stickers'),
    (N'mypromotionalpens.com/Custom_Bookmarks',N'mypromotionalpens.com/Bookmarks'),
    (N'mypromotionalpens.com/Custom_Notepads',N'mypromotionalpens.com/Notepads'),
    (N'mypromotionalpens.com/Custom_Sticky-Notes',N'mypromotionalpens.com/Sticky-Notes'),
    (N'mypromotionalpens.com/Custom_Napkins',N'mypromotionalpens.com/Napkins'),
    (N'mypromotionalpens.com/Custom_Tape-Flags',N'mypromotionalpens.com/Tape-Flags'),
    (N'mypromotionalpens.com/Custom_Labels',N'mypromotionalpens.com/Labels'),
    (N'mypromotionalpens.com/Custom_Pamphlets',N'mypromotionalpens.com/Pamphlets'),
    (N'mypromotionalpens.com/Custom_Pencil-Sharpeners',N'mypromotionalpens.com/Pencil-Sharpeners'),
    (N'mypromotionalpens.com/Custom_Rulers',N'mypromotionalpens.com/Rulers'),
    (N'mypromotionalpens.com/Custom_Padfolios',N'mypromotionalpens.com/Padfolios'),
    (N'mypromotionalpens.com/Custom_Notebooks-Jotters',N'mypromotionalpens.com/Notebooks-Jotters'),
    (N'mypromotionalpens.com/Custom_Notebooks-Padfolios',N'mypromotionalpens.com/Notebooks-Padfolios'),
    (N'mypromotionalpens.com/Custom_Memo-Pads',N'mypromotionalpens.com/Memo-Pads'),
    (N'mypromotionalpens.com/Custom_Magnetic-Memo-Pads',N'mypromotionalpens.com/Magnetic-Memo-Pads'),
    (N'mypromotionalpens.com/Custom_Journals',N'mypromotionalpens.com/Journals'),
    (N'mypromotionalpens.com/Custom_Folders',N'mypromotionalpens.com/Folders'),
    (N'mypromotionalpens.com/Custom_Tablet-Case-Holder',N'mypromotionalpens.com/Tablet-Case-Holder'),
    (N'mypromotionalpens.com/Custom_Clocks',N'mypromotionalpens.com/Clocks'),
    (N'mypromotionalpens.com/Custom_Coloring-Books',N'mypromotionalpens.com/Coloring-Books'),
    (N'mypromotionalpens.com/Custom_Paperweights',N'mypromotionalpens.com/Paperweights'),
    (N'mypromotionalpens.com/Custom_Mouses',N'mypromotionalpens.com/Mouses'),
    (N'mypromotionalpens.com/Custom_Name-Badges-Holders',N'mypromotionalpens.com/Name-Badges-Holders'),
    (N'mypromotionalpens.com/Custom_Badge-Holders',N'mypromotionalpens.com/Badge-Holders'),
    (N'mypromotionalpens.com/Custom_Convention-Type',N'mypromotionalpens.com/Convention-Type'),
    (N'mypromotionalpens.com/Custom_Retractable-Badge-Holders',N'mypromotionalpens.com/Retractable-Badge-Holders'),
    (N'mypromotionalpens.com/Custom_Calendars',N'mypromotionalpens.com/Calendars'),
    (N'mypromotionalpens.com/Custom_Popsockets',N'mypromotionalpens.com/Popsockets'),
    (N'mypromotionalpens.com/Custom_Book-Covers',N'mypromotionalpens.com/Book-Covers'),
    (N'mypromotionalpens.com/Custom_Boxes',N'mypromotionalpens.com/Boxes'),
    (N'mypromotionalpens.com/Custom_Business-Cards',N'mypromotionalpens.com/Business-Cards'),
    (N'mypromotionalpens.com/Custom_Memo-Pad-Paper-Holders',N'mypromotionalpens.com/Memo-Pad-Paper-Holders'),
    (N'mypromotionalpens.com/Custom_Binder-Accessories',N'mypromotionalpens.com/Binder-Accessories'),
    (N'mypromotionalpens.com/Custom_Binders',N'mypromotionalpens.com/Binders'),
    (N'mypromotionalpens.com/Custom_Loose-leaf-Binders',N'mypromotionalpens.com/Loose-leaf-Binders'),
    (N'mypromotionalpens.com/Custom_Clipboards',N'mypromotionalpens.com/Clipboards'),
    (N'mypromotionalpens.com/Custom_Portfolios',N'mypromotionalpens.com/Portfolios'),
    (N'mypromotionalpens.com/Custom_Tablecloths-Tablecloth-Sets',N'mypromotionalpens.com/Tablecloths-Tablecloth-Sets'),
    (N'mypromotionalpens.com/Custom_Business-Card-Cases',N'mypromotionalpens.com/Business-Card-Cases'),
    (N'mypromotionalpens.com/Custom_Magnets',N'mypromotionalpens.com/Magnets'),
    (N'mypromotionalpens.com/Custom_Business-Card-Magnets',N'mypromotionalpens.com/Business-Card-Magnets'),
    (N'mypromotionalpens.com/Custom_Button-Magnets',N'mypromotionalpens.com/Button-Magnets'),
    (N'mypromotionalpens.com/Custom_Car-Outdoor-Magnets',N'mypromotionalpens.com/Car-Outdoor-Magnets'),
    (N'mypromotionalpens.com/Custom_mypromotionalpens.com/Custom_Shape-Magnets',N'mypromotionalpens.com/mypromotionalpens.com/Shape-Magnets'),
    (N'mypromotionalpens.com/Custom_Animal-Magnets',N'mypromotionalpens.com/Animal-Magnets'),
    (N'mypromotionalpens.com/Custom_Awareness-Magnets',N'mypromotionalpens.com/Awareness-Magnets'),
    (N'mypromotionalpens.com/Custom_Calendar-Magnets',N'mypromotionalpens.com/Calendar-Magnets'),
    (N'mypromotionalpens.com/Custom_Dry-Erase-Magnets',N'mypromotionalpens.com/Dry-Erase-Magnets'),
    (N'mypromotionalpens.com/Custom_Picture-Frame-Magnets',N'mypromotionalpens.com/Picture-Frame-Magnets'),
    (N'mypromotionalpens.com/Custom_Schedule-Magnets',N'mypromotionalpens.com/Schedule-Magnets'),
    (N'mypromotionalpens.com/Custom_Car-Magnets',N'mypromotionalpens.com/Car-Magnets'),
    (N'mypromotionalpens.com/Custom_Fridge-Magnets',N'mypromotionalpens.com/Fridge-Magnets'),
    (N'mypromotionalpens.com/Custom_Clip-Magnets',N'mypromotionalpens.com/Clip-Magnets'),
    (N'mypromotionalpens.com/Custom_Outdoor-Leisure',N'mypromotionalpens.com/Outdoor-Leisure'),
    (N'mypromotionalpens.com/Custom_Towels',N'mypromotionalpens.com/Towels'),
    (N'mypromotionalpens.com/Custom_Hand-Fans',N'mypromotionalpens.com/Hand-Fans'),
    (N'mypromotionalpens.com/Custom_Bobbers',N'mypromotionalpens.com/Bobbers'),
    (N'mypromotionalpens.com/Custom_Sunglasses',N'mypromotionalpens.com/Sunglasses'),
    (N'mypromotionalpens.com/Custom_Umbrellas',N'mypromotionalpens.com/Umbrellas'),
    (N'mypromotionalpens.com/Custom_Folding-Umbrellas',N'mypromotionalpens.com/Folding-Umbrellas'),
    (N'mypromotionalpens.com/Custom_Golf-Umbrellas',N'mypromotionalpens.com/Golf-Umbrellas'),
    (N'mypromotionalpens.com/Custom_Golf',N'mypromotionalpens.com/Golf'),
    (N'mypromotionalpens.com/Custom_Golf-Balls',N'mypromotionalpens.com/Golf-Balls'),
    (N'mypromotionalpens.com/Custom_Divot-Repair-Other-Tools',N'mypromotionalpens.com/Divot-Repair-Other-Tools'),
    (N'mypromotionalpens.com/Custom_Golf-Bags',N'mypromotionalpens.com/Golf-Bags'),
    (N'mypromotionalpens.com/Custom_Golf-Tees',N'mypromotionalpens.com/Golf-Tees'),
    (N'mypromotionalpens.com/Custom_Golf-Item-Bundles-Kits',N'mypromotionalpens.com/Golf-Item-Bundles-Kits'),
    (N'mypromotionalpens.com/Custom_Golf-Shoe-Bags',N'mypromotionalpens.com/Golf-Shoe-Bags'),
    (N'mypromotionalpens.com/Custom_Golf-Gloves',N'mypromotionalpens.com/Golf-Gloves'),
    (N'mypromotionalpens.com/Custom_Golf-Towels',N'mypromotionalpens.com/Golf-Towels'),
    (N'mypromotionalpens.com/Custom_Golf-Ball-Markers',N'mypromotionalpens.com/Golf-Ball-Markers'),
    (N'mypromotionalpens.com/Custom_Golf-Shoes',N'mypromotionalpens.com/Golf-Shoes'),
    (N'mypromotionalpens.com/Custom_Golf-Clubs',N'mypromotionalpens.com/Golf-Clubs'),
    (N'mypromotionalpens.com/Custom_Chairs',N'mypromotionalpens.com/Chairs'),
    (N'mypromotionalpens.com/Custom_Compasses',N'mypromotionalpens.com/Compasses'),
    (N'mypromotionalpens.com/Custom_Whistles',N'mypromotionalpens.com/Whistles'),
    (N'mypromotionalpens.com/Custom_Blankets',N'mypromotionalpens.com/Blankets'),
    (N'mypromotionalpens.com/Custom_BBQ-Accessories',N'mypromotionalpens.com/BBQ-Accessories'),
    (N'mypromotionalpens.com/Custom_Backpack-Kits',N'mypromotionalpens.com/Backpack-Kits'),
    (N'mypromotionalpens.com/Custom_Beach-Balls',N'mypromotionalpens.com/Beach-Balls'),
    (N'mypromotionalpens.com/Custom_Grills',N'mypromotionalpens.com/Grills'),
    (N'mypromotionalpens.com/Custom_Insect-Repellents-Exterminators',N'mypromotionalpens.com/Insect-Repellents-Exterminators'),
    (N'mypromotionalpens.com/Custom_Picnic-Baskets-Kits',N'mypromotionalpens.com/Picnic-Baskets-Kits'),
    (N'mypromotionalpens.com/Custom_Beach-Accessories',N'mypromotionalpens.com/Beach-Accessories'),
    (N'mypromotionalpens.com/Custom_School-Spirit',N'mypromotionalpens.com/School-Spirit'),
    (N'mypromotionalpens.com/Custom_Window-Flags',N'mypromotionalpens.com/Window-Flags'),
    (N'mypromotionalpens.com/Custom_Megaphones',N'mypromotionalpens.com/Megaphones');
	
INSERT INTO categories_redirect_from VALUES
    (N'mypromotionalpens.com/Custom_Sports-Balls',N'mypromotionalpens.com/Sports-Balls'),
    (N'mypromotionalpens.com/Custom_Basketballs',N'mypromotionalpens.com/Basketballs'),
    (N'mypromotionalpens.com/Custom_Footballs',N'mypromotionalpens.com/Footballs'),
    (N'mypromotionalpens.com/Custom_Volleyballs',N'mypromotionalpens.com/Volleyballs'),
    (N'mypromotionalpens.com/Custom_Stadium-Cushions',N'mypromotionalpens.com/Stadium-Cushions'),
    (N'mypromotionalpens.com/Custom_#1-Foam-Hand-Mitts',N'mypromotionalpens.com/#1-Foam-Hand-Mitts'),
    (N'mypromotionalpens.com/Custom_Pom-Poms',N'mypromotionalpens.com/Pom-Poms'),
    (N'mypromotionalpens.com/Custom_Bookmarks',N'mypromotionalpens.com/Bookmarks'),
    (N'mypromotionalpens.com/Custom_Noisemakers',N'mypromotionalpens.com/Noisemakers'),
    (N'mypromotionalpens.com/Custom_Pennants',N'mypromotionalpens.com/Pennants'),
    (N'mypromotionalpens.com/Custom_Auto,-Home-Tools',N'mypromotionalpens.com/Auto,-Home-Tools'),
    (N'mypromotionalpens.com/Custom_Keychains',N'mypromotionalpens.com/Keychains'),
    (N'mypromotionalpens.com/Custom_Floaty-Keychains',N'mypromotionalpens.com/Floaty-Keychains'),
    (N'mypromotionalpens.com/Custom_Flashlight-Keychains',N'mypromotionalpens.com/Flashlight-Keychains'),
    (N'mypromotionalpens.com/Custom_Key-Fobs',N'mypromotionalpens.com/Key-Fobs'),
    (N'mypromotionalpens.com/Custom_Bottle-Opener-Keychains',N'mypromotionalpens.com/Bottle-Opener-Keychains'),
    (N'mypromotionalpens.com/Custom_Carabiner-Keychains',N'mypromotionalpens.com/Carabiner-Keychains'),
    (N'mypromotionalpens.com/Custom_Tool-Keychains',N'mypromotionalpens.com/Tool-Keychains'),
    (N'mypromotionalpens.com/Custom_Car-Air-Fresheners',N'mypromotionalpens.com/Car-Air-Fresheners'),
    (N'mypromotionalpens.com/Custom_Travel',N'mypromotionalpens.com/Travel'),
    (N'mypromotionalpens.com/Custom_Luggage-Tags',N'mypromotionalpens.com/Luggage-Tags'),
    (N'mypromotionalpens.com/Custom_Compacts-Pocket-Mirrors',N'mypromotionalpens.com/Compacts-Pocket-Mirrors'),
    (N'mypromotionalpens.com/Custom_Luggage-Straps',N'mypromotionalpens.com/Luggage-Straps'),
    (N'mypromotionalpens.com/Custom_Travel-Amenities',N'mypromotionalpens.com/Travel-Amenities'),
    (N'mypromotionalpens.com/Custom_Tools',N'mypromotionalpens.com/Tools'),
    (N'mypromotionalpens.com/Custom_Tire-Gauges',N'mypromotionalpens.com/Tire-Gauges'),
    (N'mypromotionalpens.com/Custom_Flashlights',N'mypromotionalpens.com/Flashlights'),
    (N'mypromotionalpens.com/Custom_Flashing-Lights',N'mypromotionalpens.com/Flashing-Lights'),
    (N'mypromotionalpens.com/Custom_Night-Lights',N'mypromotionalpens.com/Night-Lights'),
    (N'mypromotionalpens.com/Custom_Carabiners',N'mypromotionalpens.com/Carabiners'),
    (N'mypromotionalpens.com/Custom_Knives',N'mypromotionalpens.com/Knives'),
    (N'mypromotionalpens.com/Custom_Tape-Measures',N'mypromotionalpens.com/Tape-Measures'),
    (N'mypromotionalpens.com/Custom_Levels',N'mypromotionalpens.com/Levels'),
    (N'mypromotionalpens.com/Custom_Ice-Scrapers',N'mypromotionalpens.com/Ice-Scrapers'),
    (N'mypromotionalpens.com/Custom_Pliers',N'mypromotionalpens.com/Pliers'),
    (N'mypromotionalpens.com/Custom_Chisels',N'mypromotionalpens.com/Chisels'),
    (N'mypromotionalpens.com/Custom_Screwdrivers',N'mypromotionalpens.com/Screwdrivers'),
    (N'mypromotionalpens.com/Custom_Pocket-Screw-Drivers',N'mypromotionalpens.com/Pocket-Screw-Drivers'),
    (N'mypromotionalpens.com/Custom_Screwdrivers-Kits',N'mypromotionalpens.com/Screwdrivers-Kits'),
    (N'mypromotionalpens.com/Custom_Miniature-Screwdrivers',N'mypromotionalpens.com/Miniature-Screwdrivers'),
    (N'mypromotionalpens.com/Custom_GPS-Accessories',N'mypromotionalpens.com/GPS-Accessories'),
    (N'mypromotionalpens.com/Custom_Lanterns',N'mypromotionalpens.com/Lanterns'),
    (N'mypromotionalpens.com/Custom_Magnifiers',N'mypromotionalpens.com/Magnifiers'),
    (N'mypromotionalpens.com/Custom_Combination-Magnifiers',N'mypromotionalpens.com/Combination-Magnifiers'),
    (N'mypromotionalpens.com/Custom_Scales',N'mypromotionalpens.com/Scales'),
    (N'mypromotionalpens.com/Custom_Scrapers',N'mypromotionalpens.com/Scrapers'),
    (N'mypromotionalpens.com/Custom_Tool-Kits',N'mypromotionalpens.com/Tool-Kits'),
    (N'mypromotionalpens.com/Custom_Combo-Tools',N'mypromotionalpens.com/Combo-Tools'),
    (N'mypromotionalpens.com/Custom_Wrenches',N'mypromotionalpens.com/Wrenches'),
    (N'mypromotionalpens.com/Custom_Auto-Safety-Tools',N'mypromotionalpens.com/Auto-Safety-Tools'),
    (N'mypromotionalpens.com/Custom_Emergency',N'mypromotionalpens.com/Emergency'),
    (N'mypromotionalpens.com/Custom_Car-Care',N'mypromotionalpens.com/Car-Care'),
    (N'mypromotionalpens.com/Custom_Car-Organizers-Storage',N'mypromotionalpens.com/Car-Organizers-Storage'),
    (N'mypromotionalpens.com/Custom_Picture-Frames',N'mypromotionalpens.com/Picture-Frames'),
    (N'mypromotionalpens.com/Custom_Combination-Picture-Frames',N'mypromotionalpens.com/Combination-Picture-Frames'),
    (N'mypromotionalpens.com/Custom_Pet-Accessories',N'mypromotionalpens.com/Pet-Accessories'),
    (N'mypromotionalpens.com/Custom_Watches',N'mypromotionalpens.com/Watches'),
    (N'mypromotionalpens.com/Custom_Digital-Wrist-Watches',N'mypromotionalpens.com/Digital-Wrist-Watches'),
    (N'mypromotionalpens.com/Custom_Gift-Baskets-Sets',N'mypromotionalpens.com/Gift-Baskets-Sets'),
    (N'mypromotionalpens.com/Custom_Back-Scratchers',N'mypromotionalpens.com/Back-Scratchers'),
    (N'mypromotionalpens.com/Custom_Piggy-Banks',N'mypromotionalpens.com/Piggy-Banks'),
    (N'mypromotionalpens.com/Custom_Bells',N'mypromotionalpens.com/Bells'),
    (N'mypromotionalpens.com/Custom_Buckets',N'mypromotionalpens.com/Buckets'),
    (N'mypromotionalpens.com/Custom_Card-Sleeves',N'mypromotionalpens.com/Card-Sleeves'),
    (N'mypromotionalpens.com/Custom_Credit-Card-Sleeves',N'mypromotionalpens.com/Credit-Card-Sleeves'),
    (N'mypromotionalpens.com/Custom_Rfid-Sleeves',N'mypromotionalpens.com/Rfid-Sleeves'),
    (N'mypromotionalpens.com/Custom_Coin-Holders',N'mypromotionalpens.com/Coin-Holders'),
    (N'mypromotionalpens.com/Custom_Coin-Purses',N'mypromotionalpens.com/Coin-Purses'),
    (N'mypromotionalpens.com/Custom_Covers',N'mypromotionalpens.com/Covers'),
    (N'mypromotionalpens.com/Custom_Eyeglasses',N'mypromotionalpens.com/Eyeglasses'),
    (N'mypromotionalpens.com/Custom_Eyeglass-Cases-Holders',N'mypromotionalpens.com/Eyeglass-Cases-Holders'),
    (N'mypromotionalpens.com/Custom_Eyeglass-Cleaners',N'mypromotionalpens.com/Eyeglass-Cleaners'),
    (N'mypromotionalpens.com/Custom_Globes',N'mypromotionalpens.com/Globes'),
    (N'mypromotionalpens.com/Custom_Identity-Protection-Products',N'mypromotionalpens.com/Identity-Protection-Products'),
    (N'mypromotionalpens.com/Custom_License-Holders',N'mypromotionalpens.com/License-Holders'),
    (N'mypromotionalpens.com/Custom_Credit-Card-Cases',N'mypromotionalpens.com/Credit-Card-Cases'),
    (N'mypromotionalpens.com/Custom_Party-Products',N'mypromotionalpens.com/Party-Products'),
    (N'mypromotionalpens.com/Custom_Balloons',N'mypromotionalpens.com/Balloons'),
    (N'mypromotionalpens.com/Custom_Party-Favors',N'mypromotionalpens.com/Party-Favors'),
    (N'mypromotionalpens.com/Custom_Medallions',N'mypromotionalpens.com/Medallions'),
    (N'mypromotionalpens.com/Custom_Passport-Cases',N'mypromotionalpens.com/Passport-Cases'),
    (N'mypromotionalpens.com/Custom_Household-Items',N'mypromotionalpens.com/Household-Items'),
    (N'mypromotionalpens.com/Custom_Book-Lights',N'mypromotionalpens.com/Book-Lights'),
    (N'mypromotionalpens.com/Custom_Coolers',N'mypromotionalpens.com/Coolers'),
    (N'mypromotionalpens.com/Custom_Earplugs',N'mypromotionalpens.com/Earplugs'),
    (N'mypromotionalpens.com/Custom_Fans',N'mypromotionalpens.com/Fans'),
    (N'mypromotionalpens.com/Custom_Usb-Fans',N'mypromotionalpens.com/Usb-Fans'),
    (N'mypromotionalpens.com/Custom_Hair-Brushes',N'mypromotionalpens.com/Hair-Brushes'),
    (N'mypromotionalpens.com/Custom_Combs',N'mypromotionalpens.com/Combs'),
    (N'mypromotionalpens.com/Custom_Hooks',N'mypromotionalpens.com/Hooks'),
    (N'mypromotionalpens.com/Custom_Laundry-Aids',N'mypromotionalpens.com/Laundry-Aids'),
    (N'mypromotionalpens.com/Custom_Light-Bulbs',N'mypromotionalpens.com/Light-Bulbs'),
    (N'mypromotionalpens.com/Custom_Lint-Removers',N'mypromotionalpens.com/Lint-Removers'),
    (N'mypromotionalpens.com/Custom_Ornaments',N'mypromotionalpens.com/Ornaments'),
    (N'mypromotionalpens.com/Custom_Christmas-Tree',N'mypromotionalpens.com/Christmas-Tree'),
    (N'mypromotionalpens.com/Custom_Photography-Darkroom-Accessories',N'mypromotionalpens.com/Photography-Darkroom-Accessories'),
    (N'mypromotionalpens.com/Custom_Pillows',N'mypromotionalpens.com/Pillows'),
    (N'mypromotionalpens.com/Custom_Reflectors',N'mypromotionalpens.com/Reflectors'),
    (N'mypromotionalpens.com/Custom_Ropes',N'mypromotionalpens.com/Ropes'),
    (N'mypromotionalpens.com/Custom_Sewing-Accessories-Kits',N'mypromotionalpens.com/Sewing-Accessories-Kits'),
    (N'mypromotionalpens.com/Custom_Shoe-Shine-Kits',N'mypromotionalpens.com/Shoe-Shine-Kits'),
    (N'mypromotionalpens.com/Custom_Shoehorns',N'mypromotionalpens.com/Shoehorns'),
    (N'mypromotionalpens.com/Custom_Tins',N'mypromotionalpens.com/Tins'),
    (N'mypromotionalpens.com/Custom_Tissues',N'mypromotionalpens.com/Tissues'),
    (N'mypromotionalpens.com/Custom_Towelettes',N'mypromotionalpens.com/Towelettes'),
    (N'mypromotionalpens.com/Custom_Towels',N'mypromotionalpens.com/Towels'),
    (N'mypromotionalpens.com/Custom_Beach-Towels',N'mypromotionalpens.com/Beach-Towels'),
    (N'mypromotionalpens.com/Custom_Sports-Towels',N'mypromotionalpens.com/Sports-Towels'),
    (N'mypromotionalpens.com/Custom_Zipper-Pullers',N'mypromotionalpens.com/Zipper-Pullers'),
    (N'mypromotionalpens.com/Custom_Kitchen-Accessories',N'mypromotionalpens.com/Kitchen-Accessories'),
    (N'mypromotionalpens.com/Custom_Bag-Clips-Sealers',N'mypromotionalpens.com/Bag-Clips-Sealers'),
    (N'mypromotionalpens.com/Custom_Chip-Clips',N'mypromotionalpens.com/Chip-Clips'),
    (N'mypromotionalpens.com/Custom_Magnetic-Clips',N'mypromotionalpens.com/Magnetic-Clips'),
    (N'mypromotionalpens.com/Custom_Bar-Accessories',N'mypromotionalpens.com/Bar-Accessories'),
    (N'mypromotionalpens.com/Custom_Boards',N'mypromotionalpens.com/Boards'),
    (N'mypromotionalpens.com/Custom_Carving-Chopping-Boards',N'mypromotionalpens.com/Carving-Chopping-Boards'),
    (N'mypromotionalpens.com/Custom_Bowls',N'mypromotionalpens.com/Bowls'),
    (N'mypromotionalpens.com/Custom_Corkscrews',N'mypromotionalpens.com/Corkscrews'),
    (N'mypromotionalpens.com/Custom_Grippers',N'mypromotionalpens.com/Grippers'),
    (N'mypromotionalpens.com/Custom_Measuring-Cups',N'mypromotionalpens.com/Measuring-Cups'),
    (N'mypromotionalpens.com/Custom_Measuring-Spoons',N'mypromotionalpens.com/Measuring-Spoons'),
    (N'mypromotionalpens.com/Custom_Pitchers',N'mypromotionalpens.com/Pitchers'),
    (N'mypromotionalpens.com/Custom_Plates',N'mypromotionalpens.com/Plates'),
    (N'mypromotionalpens.com/Custom_Salad-Sets',N'mypromotionalpens.com/Salad-Sets'),
    (N'mypromotionalpens.com/Custom_Salt-Pepper-Shakers-And-Mills',N'mypromotionalpens.com/Salt-Pepper-Shakers-And-Mills'),
    (N'mypromotionalpens.com/Custom_Kitchen-Tools',N'mypromotionalpens.com/Kitchen-Tools'),
    (N'mypromotionalpens.com/Custom_Cutters',N'mypromotionalpens.com/Cutters'),
    (N'mypromotionalpens.com/Custom_Forks-Spoons',N'mypromotionalpens.com/Forks-Spoons'),
    (N'mypromotionalpens.com/Custom_Pizza-Accessories',N'mypromotionalpens.com/Pizza-Accessories'),
    (N'mypromotionalpens.com/Custom_Spatulas-Spreaders',N'mypromotionalpens.com/Spatulas-Spreaders'),
    (N'mypromotionalpens.com/Custom_Strainers',N'mypromotionalpens.com/Strainers'),
    (N'mypromotionalpens.com/Custom_Whisks',N'mypromotionalpens.com/Whisks'),
    (N'mypromotionalpens.com/Custom_Chopsticks',N'mypromotionalpens.com/Chopsticks'),
    (N'mypromotionalpens.com/Custom_Serving-Trays',N'mypromotionalpens.com/Serving-Trays'),
    (N'mypromotionalpens.com/Custom_Wine-Accessories',N'mypromotionalpens.com/Wine-Accessories'),
    (N'mypromotionalpens.com/Custom_Food-Storage',N'mypromotionalpens.com/Food-Storage'),
    (N'mypromotionalpens.com/Custom_Jar-Openers',N'mypromotionalpens.com/Jar-Openers'),
    (N'mypromotionalpens.com/Custom_Coins-tokens-Medallions',N'mypromotionalpens.com/Coins-tokens-Medallions'),
    (N'mypromotionalpens.com/Custom_License-Plates',N'mypromotionalpens.com/License-Plates'),
    (N'mypromotionalpens.com/Custom_License-Plate-Frames',N'mypromotionalpens.com/License-Plate-Frames'),
    (N'mypromotionalpens.com/Custom_Technology',N'mypromotionalpens.com/Technology'),
    (N'mypromotionalpens.com/Custom_Earbuds,-Headphones-Speakers',N'mypromotionalpens.com/Earbuds,-Headphones-Speakers'),
    (N'mypromotionalpens.com/Custom_Wireless-Speakers',N'mypromotionalpens.com/Wireless-Speakers'),
    (N'mypromotionalpens.com/Custom_Wireless-Blue-Tooth-Earbuds',N'mypromotionalpens.com/Wireless-Blue-Tooth-Earbuds'),
    (N'mypromotionalpens.com/Custom_USB-Flash-Drives',N'mypromotionalpens.com/USB-Flash-Drives'),
    (N'mypromotionalpens.com/Custom_USB-Adapters',N'mypromotionalpens.com/USB-Adapters'),
    (N'mypromotionalpens.com/Custom_Electronic-Accessories',N'mypromotionalpens.com/Electronic-Accessories'),
    (N'mypromotionalpens.com/Custom_Smartphone-Accessories',N'mypromotionalpens.com/Smartphone-Accessories'),
    (N'mypromotionalpens.com/Custom_Mobile-Accessories',N'mypromotionalpens.com/Mobile-Accessories'),
    (N'mypromotionalpens.com/Custom_Selfie-Sticks',N'mypromotionalpens.com/Selfie-Sticks'),
    (N'mypromotionalpens.com/Custom_Cables-Cords',N'mypromotionalpens.com/Cables-Cords'),
    (N'mypromotionalpens.com/Custom_Power-Banks',N'mypromotionalpens.com/Power-Banks'),
    (N'mypromotionalpens.com/Custom_Mouses',N'mypromotionalpens.com/Mouses'),
    (N'mypromotionalpens.com/Custom_Pedometers',N'mypromotionalpens.com/Pedometers'),
    (N'mypromotionalpens.com/Custom_Calculators',N'mypromotionalpens.com/Calculators'),
    (N'mypromotionalpens.com/Custom_Phone-Cases-Holders',N'mypromotionalpens.com/Phone-Cases-Holders'),
    (N'mypromotionalpens.com/Custom_Palms/pda-Accessories',N'mypromotionalpens.com/Palms/pda-Accessories'),
    (N'mypromotionalpens.com/Custom_Computer-Accessories',N'mypromotionalpens.com/Computer-Accessories'),
    (N'mypromotionalpens.com/Custom_Cleaners',N'mypromotionalpens.com/Cleaners'),
    (N'mypromotionalpens.com/Custom_Microfiber-Cloths',N'mypromotionalpens.com/Microfiber-Cloths'),
    (N'mypromotionalpens.com/Custom_Wireless-Qi-Charger',N'mypromotionalpens.com/Wireless-Qi-Charger'),
    (N'mypromotionalpens.com/Custom_Trackers',N'mypromotionalpens.com/Trackers'),
    (N'mypromotionalpens.com/Custom_Car-Chargers',N'mypromotionalpens.com/Car-Chargers'),
    (N'mypromotionalpens.com/Custom_Toys-Fun-Stuff',N'mypromotionalpens.com/Toys-Fun-Stuff'),
    (N'mypromotionalpens.com/Custom_Dog-Tags',N'mypromotionalpens.com/Dog-Tags'),
    (N'mypromotionalpens.com/Custom_Tattoos',N'mypromotionalpens.com/Tattoos'),
    (N'mypromotionalpens.com/Custom_Temporary-Tattoos-Waterless',N'mypromotionalpens.com/Temporary-Tattoos-Waterless'),
    (N'mypromotionalpens.com/Custom_Temporary-Tattoos',N'mypromotionalpens.com/Temporary-Tattoos'),
    (N'mypromotionalpens.com/Custom_Fingernail-Tattoos',N'mypromotionalpens.com/Fingernail-Tattoos'),
    (N'mypromotionalpens.com/Custom_Frisbee-Flyers',N'mypromotionalpens.com/Frisbee-Flyers'),
    (N'mypromotionalpens.com/Custom_Puzzles-Tricks',N'mypromotionalpens.com/Puzzles-Tricks'),
    (N'mypromotionalpens.com/Custom_Basketball-Hoop-Set',N'mypromotionalpens.com/Basketball-Hoop-Set'),
    (N'mypromotionalpens.com/Custom_Games',N'mypromotionalpens.com/Games'),
    (N'mypromotionalpens.com/Custom_Playing-Cards',N'mypromotionalpens.com/Playing-Cards'),
    (N'mypromotionalpens.com/Custom_Playing-Card-Cases',N'mypromotionalpens.com/Playing-Card-Cases'),
    (N'mypromotionalpens.com/Custom_Poker-Chips',N'mypromotionalpens.com/Poker-Chips'),
    (N'mypromotionalpens.com/Custom_Stuffed-Animals-Toys',N'mypromotionalpens.com/Stuffed-Animals-Toys'),
    (N'mypromotionalpens.com/Custom_Tops-Spinners',N'mypromotionalpens.com/Tops-Spinners'),
    (N'mypromotionalpens.com/Custom_Fidget-Spinners',N'mypromotionalpens.com/Fidget-Spinners'),
    (N'mypromotionalpens.com/Custom_Viewers',N'mypromotionalpens.com/Viewers'),
    (N'mypromotionalpens.com/Custom_3d-Viewers',N'mypromotionalpens.com/3d-Viewers'),
    (N'mypromotionalpens.com/Custom_Executive-Toys',N'mypromotionalpens.com/Executive-Toys'),
    (N'mypromotionalpens.com/Custom_Fidget-Cubes',N'mypromotionalpens.com/Fidget-Cubes'),
    (N'mypromotionalpens.com/Custom_Trade-Show-Signage',N'mypromotionalpens.com/Trade-Show-Signage'),
    (N'mypromotionalpens.com/Custom_Banners',N'mypromotionalpens.com/Banners'),
    (N'mypromotionalpens.com/Custom_Signs-Displays',N'mypromotionalpens.com/Signs-Displays'),
    (N'mypromotionalpens.com/Custom_Bottle-Collar',N'mypromotionalpens.com/Bottle-Collar'),
    (N'mypromotionalpens.com/Custom_Lanyards',N'mypromotionalpens.com/Lanyards'),
    (N'mypromotionalpens.com/Custom_Wellness-Safety',N'mypromotionalpens.com/Wellness-Safety'),
    (N'mypromotionalpens.com/Custom_Hand-Sanitizers',N'mypromotionalpens.com/Hand-Sanitizers'),
    (N'mypromotionalpens.com/Custom_Prescription-Bottles',N'mypromotionalpens.com/Prescription-Bottles'),
    (N'mypromotionalpens.com/Custom_Pill-Boxes-Bottles',N'mypromotionalpens.com/Pill-Boxes-Bottles'),
    (N'mypromotionalpens.com/Custom_Oral-Care',N'mypromotionalpens.com/Oral-Care'),
    (N'mypromotionalpens.com/Custom_Toothbrushes',N'mypromotionalpens.com/Toothbrushes'),
    (N'mypromotionalpens.com/Custom_Dental-Floss',N'mypromotionalpens.com/Dental-Floss'),
    (N'mypromotionalpens.com/Custom_Hour-Glass-Timers',N'mypromotionalpens.com/Hour-Glass-Timers'),
    (N'mypromotionalpens.com/Custom_Toothbrush-Holders',N'mypromotionalpens.com/Toothbrush-Holders'),
    (N'mypromotionalpens.com/Custom_Toothpaste',N'mypromotionalpens.com/Toothpaste'),
    (N'mypromotionalpens.com/Custom_Toothbrush-Cases',N'mypromotionalpens.com/Toothbrush-Cases'),
    (N'mypromotionalpens.com/Custom_Lip-Balm',N'mypromotionalpens.com/Lip-Balm'),
    (N'mypromotionalpens.com/Custom_Sunscreen',N'mypromotionalpens.com/Sunscreen'),
    (N'mypromotionalpens.com/Custom_First-Aid',N'mypromotionalpens.com/First-Aid'),
    (N'mypromotionalpens.com/Custom_Bandages',N'mypromotionalpens.com/Bandages'),
    (N'mypromotionalpens.com/Custom_Bandage-Dispensers',N'mypromotionalpens.com/Bandage-Dispensers'),
    (N'mypromotionalpens.com/Custom_Informational-Guides',N'mypromotionalpens.com/Informational-Guides'),
    (N'mypromotionalpens.com/Custom_Apothecary-Jars',N'mypromotionalpens.com/Apothecary-Jars'),
    (N'mypromotionalpens.com/Custom_Exercise-Equipment',N'mypromotionalpens.com/Exercise-Equipment'),
    (N'mypromotionalpens.com/Custom_Mats',N'mypromotionalpens.com/Mats'),
    (N'mypromotionalpens.com/Custom_Heat-Pads-Packs',N'mypromotionalpens.com/Heat-Pads-Packs'),
    (N'mypromotionalpens.com/Custom_Ice-Packs',N'mypromotionalpens.com/Ice-Packs'),
    (N'mypromotionalpens.com/Custom_Manicure-Tools',N'mypromotionalpens.com/Manicure-Tools'),
    (N'mypromotionalpens.com/Custom_Emery-Boards',N'mypromotionalpens.com/Emery-Boards'),
    (N'mypromotionalpens.com/Custom_Masks',N'mypromotionalpens.com/Masks'),
    (N'mypromotionalpens.com/Custom_Massagers',N'mypromotionalpens.com/Massagers'),
    (N'mypromotionalpens.com/Custom_Scissors-Shears',N'mypromotionalpens.com/Scissors-Shears'),
    (N'mypromotionalpens.com/Custom_Razors-Electric-Shavers',N'mypromotionalpens.com/Razors-Electric-Shavers'),
    (N'mypromotionalpens.com/Custom_Shaving-Accessories-Kits',N'mypromotionalpens.com/Shaving-Accessories-Kits'),
    (N'mypromotionalpens.com/Custom_Soap',N'mypromotionalpens.com/Soap'),
    (N'mypromotionalpens.com/Custom_Spa-Products',N'mypromotionalpens.com/Spa-Products'),
    (N'mypromotionalpens.com/Custom_Thermometers',N'mypromotionalpens.com/Thermometers'),
    (N'mypromotionalpens.com/Custom_Lotion',N'mypromotionalpens.com/Lotion'),
    (N'mypromotionalpens.com/Custom_Suntan-Lotions',N'mypromotionalpens.com/Suntan-Lotions'),
    (N'mypromotionalpens.com/Custom_Fresheners',N'mypromotionalpens.com/Fresheners'),
    (N'mypromotionalpens.com/Custom_Breath',N'mypromotionalpens.com/Breath'),
    (N'mypromotionalpens.com/Custom_Writing-Instruments',N'mypromotionalpens.com/Writing-Instruments'),
    (N'mypromotionalpens.com/Custom_Pencils',N'mypromotionalpens.com/Pencils'),
    (N'mypromotionalpens.com/Custom_Woodcase-Pencils',N'mypromotionalpens.com/Woodcase-Pencils'),
    (N'mypromotionalpens.com/Custom_Mechanical-Pencils',N'mypromotionalpens.com/Mechanical-Pencils'),
    (N'mypromotionalpens.com/Custom_Decorative-Pencils',N'mypromotionalpens.com/Decorative-Pencils'),
    (N'mypromotionalpens.com/Custom_Golf-Pencils',N'mypromotionalpens.com/Golf-Pencils'),
    (N'mypromotionalpens.com/Custom_Carpenter-Pencils',N'mypromotionalpens.com/Carpenter-Pencils'),
    (N'mypromotionalpens.com/Custom_Coloring-Pencils',N'mypromotionalpens.com/Coloring-Pencils'),
    (N'mypromotionalpens.com/Custom_Personalized-Pens',N'mypromotionalpens.com/Pens'),
    (N'mypromotionalpens.com/Custom_Retractable-Pens',N'mypromotionalpens.com/Retractable-Pens'),
    (N'mypromotionalpens.com/Custom_Metal-Pens',N'mypromotionalpens.com/Metal-Pens'),
    (N'mypromotionalpens.com/Custom_Engraved-Metal-Pens',N'mypromotionalpens.com/Engraved-Metal-Pens'),
    (N'mypromotionalpens.com/Custom_925-Sterling-Silver-Pens',N'mypromotionalpens.com/925-Sterling-Silver-Pens'),
    (N'mypromotionalpens.com/Custom_Printed-Metal-Pens',N'mypromotionalpens.com/Printed-Metal-Pens'),
    (N'mypromotionalpens.com/Custom_Hot-Stamped',N'mypromotionalpens.com/Hot-Stamped'),
    (N'mypromotionalpens.com/Custom_Novelty-Pens',N'mypromotionalpens.com/Novelty-Pens'),
    (N'mypromotionalpens.com/Custom_Mailer-Flat-Pens',N'mypromotionalpens.com/Mailer-Flat-Pens'),
    (N'mypromotionalpens.com/Custom_Bendy-Pens',N'mypromotionalpens.com/Bendy-Pens'),
    (N'mypromotionalpens.com/Custom_Giant-Jumbo-Pens',N'mypromotionalpens.com/Giant-Jumbo-Pens'),
    (N'mypromotionalpens.com/Custom_Light-Up-Pens',N'mypromotionalpens.com/Light-Up-Pens'),
    (N'mypromotionalpens.com/Custom_Gel-Pens',N'mypromotionalpens.com/Gel-Pens'),
    (N'mypromotionalpens.com/Custom_Translucent-Pens',N'mypromotionalpens.com/Translucent-Pens'),
    (N'mypromotionalpens.com/Custom_Stick-Pens',N'mypromotionalpens.com/Stick-Pens'),
    (N'mypromotionalpens.com/Custom_Executive-Pens',N'mypromotionalpens.com/Executive-Pens'),
    (N'mypromotionalpens.com/Custom_Corporate-Gift-Pens',N'mypromotionalpens.com/Corporate-Gift-Pens'),
    (N'mypromotionalpens.com/Custom_Gift-Set-Pens',N'mypromotionalpens.com/Gift-Set-Pens');
	
INSERT INTO categories_redirect_from VALUES
    (N'mypromotionalpens.com/Custom_1-Day-Rush-Pens',N'mypromotionalpens.com/1-Day-Rush-Pens'),
    (N'mypromotionalpens.com/Custom_Bargain-Pens',N'mypromotionalpens.com/Bargain-Pens'),
    (N'mypromotionalpens.com/Custom_Multi-color-Imprint-Pens',N'mypromotionalpens.com/Multi-color-Imprint-Pens'),
    (N'mypromotionalpens.com/Custom_Jumbo-Pens',N'mypromotionalpens.com/Jumbo-Pens'),
    (N'mypromotionalpens.com/Custom_Grip-Pens',N'mypromotionalpens.com/Grip-Pens'),
    (N'mypromotionalpens.com/Custom_Most-Popular-Pens',N'mypromotionalpens.com/Most-Popular-Pens'),
    (N'mypromotionalpens.com/Custom_Full-Color-Wrap-Pens',N'mypromotionalpens.com/Full-Color-Wrap-Pens'),
    (N'mypromotionalpens.com/Custom_Eco-friendly-Pens',N'mypromotionalpens.com/Eco-friendly-Pens'),
    (N'mypromotionalpens.com/Custom_Blue-Ink-Pens',N'mypromotionalpens.com/Blue-Ink-Pens'),
    (N'mypromotionalpens.com/Custom_Small-Quantity-Pens',N'mypromotionalpens.com/Small-Quantity-Pens'),
    (N'mypromotionalpens.com/Custom_Rollerball-Pens',N'mypromotionalpens.com/Rollerball-Pens'),
    (N'mypromotionalpens.com/Custom_Patriotic-Pens',N'mypromotionalpens.com/Patriotic-Pens'),
    (N'mypromotionalpens.com/Custom_Made-In-Usa-Pens',N'mypromotionalpens.com/Made-In-Usa-Pens'),
    (N'mypromotionalpens.com/Custom_Wood-Pens',N'mypromotionalpens.com/Wood-Pens'),
    (N'mypromotionalpens.com/Custom_Syringe-Pens',N'mypromotionalpens.com/Syringe-Pens'),
    (N'mypromotionalpens.com/Custom_Name-Brand-Pens',N'mypromotionalpens.com/Name-Brand-Pens'),
    (N'mypromotionalpens.com/Custom_Cross-Pens',N'mypromotionalpens.com/Cross-Pens'),
    (N'mypromotionalpens.com/Custom_Uni-ball-Pens',N'mypromotionalpens.com/Uni-ball-Pens'),
    (N'mypromotionalpens.com/Custom_Bic-Pens',N'mypromotionalpens.com/Bic-Pens'),
    (N'mypromotionalpens.com/Custom_Sharpie-Pens',N'mypromotionalpens.com/Sharpie-Pens'),
    (N'mypromotionalpens.com/Custom_Paper-Mate-Pens',N'mypromotionalpens.com/Paper-Mate-Pens'),
    (N'mypromotionalpens.com/Custom_Expo-Pens',N'mypromotionalpens.com/Expo-Pens'),
    (N'mypromotionalpens.com/Custom_Quill-Pens',N'mypromotionalpens.com/Quill-Pens'),
    (N'mypromotionalpens.com/Custom_Parker-Pens',N'mypromotionalpens.com/Parker-Pens'),
    (N'mypromotionalpens.com/Custom_Waterman-Pens',N'mypromotionalpens.com/Waterman-Pens'),
    (N'mypromotionalpens.com/Custom_Liqui-Mark-Pens',N'mypromotionalpens.com/Liqui-Mark-Pens'),
    (N'mypromotionalpens.com/Custom_Stylus-Pens',N'mypromotionalpens.com/Stylus-Pens'),
    (N'mypromotionalpens.com/Custom_Awareness-Pens',N'mypromotionalpens.com/Awareness-Pens'),
    (N'mypromotionalpens.com/Custom_Breast-Cancer-Awareness-Pens',N'mypromotionalpens.com/Breast-Cancer-Awareness-Pens'),
    (N'mypromotionalpens.com/Custom_Fountain-Pens',N'mypromotionalpens.com/Fountain-Pens'),
    (N'mypromotionalpens.com/Custom_Multi-function-Pens',N'mypromotionalpens.com/Multi-function-Pens'),
    (N'mypromotionalpens.com/Custom_Lipstick-Pens',N'mypromotionalpens.com/Lipstick-Pens'),
    (N'mypromotionalpens.com/Custom_Pen-Packaging',N'mypromotionalpens.com/Pen-Packaging'),
    (N'mypromotionalpens.com/Custom_No-Minimum-Pens',N'mypromotionalpens.com/No-Minimum-Pens'),
    (N'mypromotionalpens.com/Custom_Stylus-Flashlight-Pens',N'mypromotionalpens.com/Stylus-Flashlight-Pens'),
    (N'mypromotionalpens.com/Custom_Rubberized-Pens',N'mypromotionalpens.com/Rubberized-Pens'),
    (N'mypromotionalpens.com/Custom_Church-Pens',N'mypromotionalpens.com/Church-Pens'),
    (N'mypromotionalpens.com/Custom_Highlighters-Markers',N'mypromotionalpens.com/Highlighters-Markers'),
    (N'mypromotionalpens.com/Custom_Highlighters',N'mypromotionalpens.com/Highlighters'),
    (N'mypromotionalpens.com/Custom_Markers',N'mypromotionalpens.com/Markers'),
    (N'mypromotionalpens.com/Custom_Felt-Tip-Pens',N'mypromotionalpens.com/Felt-Tip-Pens'),
    (N'mypromotionalpens.com/Custom_Crayons',N'mypromotionalpens.com/Crayons'),
    (N'mypromotionalpens.com/Custom_Boxes-Cases',N'mypromotionalpens.com/Boxes-Cases'),
    (N'mypromotionalpens.com/Custom_Erasers',N'mypromotionalpens.com/Erasers');






DECLARE @MyCursor CURSOR,
		@redirectFromUrl nvarchar(128), 
		@redirectToUrl nvarchar(128),
		@cleanedRedirectFromUrl nvarchar(128), 
		@cleanedRedirectToUrl nvarchar(128),
		@url nvarchar(128) = 'mypromotionalpens.com/';

BEGIN
	SET @MyCursor = CURSOR 
	FOR select [Redirect_From_URL],[Redirect_To_URL] from [categories_redirect_from];    

	OPEN @MyCursor 
	FETCH NEXT FROM @MyCursor 
	INTO @redirectFromUrl, @redirectToUrl

	WHILE @@FETCH_STATUS = 0
	BEGIN

		/*  insert here!!!  */
		set @cleanedRedirectFromUrl = REPLACE(@redirectFromUrl, @url, '');
		set @cleanedRedirectToUrl = REPLACE(@redirectToUrl, @url, '/');

		if ((select count(*) from [dbo].[CustomRedirect] where [Alias] = @cleanedRedirectFromUrl) > 0)
		begin
			/* update URL link */
			update [dbo].[CustomRedirect]
			set [RedirectTo] = @cleanedRedirectToUrl
			where [Alias] = @cleanedRedirectFromUrl; 
		end
		else
		begin
			/* create new URL link */
			insert into [dbo].[CustomRedirect]
			values (@cleanedRedirectFromUrl, @cleanedRedirectToUrl, 1, 1);
		end

		FETCH NEXT FROM @MyCursor 
		INTO @redirectFromUrl, @redirectToUrl 
	END; 

	CLOSE @MyCursor ;
	DEALLOCATE @MyCursor;
END;


-- Updated 2/15/2022 to remove "Personalized" data
DECLARE @MyCursor CURSOR,
		@redirectFromUrl nvarchar(128), 
		@redirectToUrl nvarchar(128),
		@cleanedRedirectFromUrl nvarchar(128), 
		@cleanedRedirectToUrl nvarchar(128),
		@url nvarchar(128) = 'mypromotionalpens.com/',
		@urlWithCustom nvarchar(128) = 'mypromotionalpens.com/Custom';

BEGIN
	SET @MyCursor = CURSOR 
	FOR select [Redirect_From_URL],[Redirect_To_URL] from [categories_redirect_from];    

	OPEN @MyCursor 
	FETCH NEXT FROM @MyCursor 
	INTO @redirectFromUrl, @redirectToUrl

	WHILE @@FETCH_STATUS = 0
	BEGIN

		/*  insert here!!!  */
		set @cleanedRedirectFromUrl = 'Personalized'+REPLACE(@redirectFromUrl, @urlWithCustom, '');
		set @cleanedRedirectToUrl = REPLACE(@redirectToUrl, @url, '/');

		if ((select count(*) from [dbo].[CustomRedirect] where [Alias] = @cleanedRedirectFromUrl) > 0)
		begin
			/* update URL link */
			update [dbo].[CustomRedirect]
			set [RedirectTo] = @cleanedRedirectToUrl
			where [Alias] = @cleanedRedirectFromUrl; 
		end
		else
		begin
			/* create new URL link */
			insert into [dbo].[CustomRedirect]
			values (@cleanedRedirectFromUrl, @cleanedRedirectToUrl, 1, 1);
		end

		FETCH NEXT FROM @MyCursor 
		INTO @redirectFromUrl, @redirectToUrl 
	END; 

	CLOSE @MyCursor ;
	DEALLOCATE @MyCursor;
END;

insert into [dbo].[CustomRedirect]
values ('Personalized_pens', 'pens', 1, 1);



-- SAV-47
update [dbo].[LocaleStringResource]
set [ResourceValue] = 'Featured Custom Products'
where ResourceName = 'HomePage.Products';

update [dbo].[LocaleStringResource]
set [ResourceValue] = 'Latest Promo Info'
where ResourceName = 'SevenSpikes.RichBlog.Public.LatestFromBlog';

update [dbo].[Topic]
set [Title] = 'Why SaveYourInk Promos?'
where [SystemName] = 'HomePageWhyUs'


-- SAV-48
update [dbo].[Topic]
set [Body] = '<div class="error-page-block"><h2>Uh Oh! The page you''re looking has been removed or does not exists. (404 Error)</h2><div class="error-page-block__button"><a href="/">Continue</a></div></div>'
where [SystemName] = 'PageNotFound'