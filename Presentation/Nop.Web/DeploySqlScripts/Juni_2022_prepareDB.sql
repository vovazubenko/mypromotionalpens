
/***************************************************/
/***************    [dbo].[Discount2]    ***********/
/***************************************************/

CREATE TABLE [dbo].[Discount2](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Name] [nvarchar](200) NOT NULL,
    [DiscountTypeId] [int] NOT NULL,
    [UsePercentage] [bit] NOT NULL,
    [DiscountPercentage] [decimal](18, 4) NOT NULL,
    [DiscountAmount] [decimal](18, 4) NOT NULL,
    [MaximumDiscountAmount] [decimal](18, 4) NULL,
    [StartDateUtc] [datetime] NULL,
    [EndDateUtc] [datetime] NULL,
    [RequiresCouponCode] [bit] NOT NULL,
    [CouponCode] [nvarchar](100) NULL,
    [IsCumulative] [bit] NOT NULL,
    [DiscountLimitationId] [int] NOT NULL,
    [LimitationTimes] [int] NOT NULL,
    [MaximumDiscountedQuantity] [int] NULL,
    [AppliedToSubCategories] [bit] NOT NULL,
    [MinimumDiscountedQuantity] [int] NULL,
    [MinimumDiscountAmount] [decimal](18, 4) NULL,
    PRIMARY KEY CLUSTERED
(
[Id] ASC
--)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


SET IDENTITY_INSERT [dbo].[Discount2] ON

insert into [dbo].[Discount2](
    [Id],
    [Name],
    [DiscountTypeId],
    [UsePercentage],
    [DiscountPercentage],
    [DiscountAmount],
    [MaximumDiscountAmount],
    [StartDateUtc],
    [EndDateUtc],
    [RequiresCouponCode],
    [CouponCode],
    [IsCumulative],
    [DiscountLimitationId],
    [LimitationTimes],
    [MaximumDiscountedQuantity],
    [AppliedToSubCategories],
    [MinimumDiscountedQuantity],
[MinimumDiscountAmount]
)
select * from [dbo].[Discount];

SET IDENTITY_INSERT [dbo].[Discount2] OFF 



/*****************************************************/
/*******  [dbo].[Discount_AppliedToCategories2]  *****/
/*****************************************************/


CREATE TABLE [dbo].[Discount_AppliedToCategories2](
    [Discount_Id] [int] NOT NULL,
    [Category_Id] [int] NOT NULL,
     PRIMARY KEY CLUSTERED
    (
    [Discount_Id] ASC,
[Category_Id] ASC
-- )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Discount_AppliedToCategories2]  WITH CHECK ADD  CONSTRAINT [Discount_AppliedToCategories_Source2] FOREIGN KEY([Discount_Id])
    REFERENCES [dbo].[Discount2] ([Id])
    ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Discount_AppliedToCategories2] CHECK CONSTRAINT [Discount_AppliedToCategories_Source2]
    GO

ALTER TABLE [dbo].[Discount_AppliedToCategories2]  WITH CHECK ADD  CONSTRAINT [Discount_AppliedToCategories_Target2] FOREIGN KEY([Category_Id])
    REFERENCES [dbo].[Category] ([Id])
    ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Discount_AppliedToCategories2] CHECK CONSTRAINT [Discount_AppliedToCategories_Target2]
GO


insert into [dbo].[Discount_AppliedToCategories2](
    [Discount_Id],
    [Category_Id]
)
select * from [dbo].[Discount_AppliedToCategories];



/*****************************************************/
/*******   [dbo].[Discount_AppliedToProducts2]   *****/
/*****************************************************/

CREATE TABLE [dbo].[Discount_AppliedToProducts2](
    [Discount_Id] [int] NOT NULL,
    [Product_Id] [int] NOT NULL,
     PRIMARY KEY CLUSTERED
    (
    [Discount_Id] ASC,
[Product_Id] ASC
--)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Discount_AppliedToProducts2]  WITH CHECK ADD  CONSTRAINT [Discount_AppliedToProducts_Source2] FOREIGN KEY([Discount_Id])
    REFERENCES [dbo].[Discount] ([Id])
    ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Discount_AppliedToProducts2] CHECK CONSTRAINT [Discount_AppliedToProducts_Source2]
    GO

ALTER TABLE [dbo].[Discount_AppliedToProducts2]  WITH CHECK ADD  CONSTRAINT [Discount_AppliedToProducts_Target2] FOREIGN KEY([Product_Id])
    REFERENCES [dbo].[Product] ([Id])
    ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Discount_AppliedToProducts2] CHECK CONSTRAINT [Discount_AppliedToProducts_Target2]
GO


insert into [dbo].[Discount_AppliedToProducts2](
    [Discount_Id],
    [Product_Id]
)
select * from [dbo].[Discount_AppliedToProducts];



/*****************************************************/
/*****************    PREPARE DB     *****************/
/*****************************************************/

/*
delete from [dbo].[Discount_AppliedToProducts];
delete from [dbo].[Discount_AppliedToCategories];
delete from [dbo].[Discount];




SET IDENTITY_INSERT [dbo].[Discount] ON

insert into [dbo].[Discount](
	[Id],
	[Name],
	[DiscountTypeId],
	[UsePercentage],
	[DiscountPercentage],
	[DiscountAmount],
	[MaximumDiscountAmount],
	[StartDateUtc],
	[EndDateUtc],
	[RequiresCouponCode],
	[CouponCode],
	[IsCumulative],
	[DiscountLimitationId],
	[LimitationTimes],
	[MaximumDiscountedQuantity],
	[AppliedToSubCategories],
	[MinimumDiscountedQuantity],
	[MinimumDiscountAmount]
)
select * from [dbo].[Discount2];

SET IDENTITY_INSERT [dbo].[Discount] OFF 


insert into [dbo].[Discount_AppliedToCategories](
	[Discount_Id],
	[Category_Id]
)
select * from [dbo].[Discount_AppliedToCategories2];



insert into [dbo].[Discount_AppliedToProducts](
    [Discount_Id],
[Product_Id]
)
select * from [dbo].[Discount_AppliedToProducts2];


*/