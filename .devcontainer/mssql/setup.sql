use restaurant;
go

DROP TABLE IF EXISTS dbo.products;
DROP TABLE IF EXISTS dbo.orders;
DROP TABLE IF EXISTS dbo.order_products;
DROP PROCEDURE IF EXISTS dbo.insert_order;

CREATE TABLE [dbo].[products] (
  [id]    UNIQUEIDENTIFIER    NOT NULL,
  [name]  NVARCHAR (50)       NOT NULL,
  [price] DECIMAL (18, 2)     NOT NULL,
  CONSTRAINT [pk_products] PRIMARY KEY CLUSTERED ([id] ASC)
);
GO

CREATE TABLE [dbo].[orders] (
  [id]                    UNIQUEIDENTIFIER NOT NULL,
  [customer_name]         NVARCHAR (50) NOT NULL,
  [customer_address]      NVARCHAR (50) NOT NULL,
  [customer_phone_number] NVARCHAR (50) NOT NULL,
  [total_price]           DECIMAL (18, 2) NOT NULL,
  [order_date]            DATETIME NOT NULL,
  [status]                NVARCHAR (50) NOT NULL,
  CONSTRAINT [pk_orders]  PRIMARY KEY CLUSTERED ([id] ASC)
);
GO

CREATE TABLE [dbo].[order_products] (
  [id]                            UNIQUEIDENTIFIER NOT NULL,
  [order_id]                      UNIQUEIDENTIFIER NOT NULL,
  [product_id]                    UNIQUEIDENTIFIER NOT NULL,
  [quantity]                      INT NOT NULL,
  [price]                         DECIMAL (18, 2) NOT NULL,
  CONSTRAINT [pk_order_products]  PRIMARY KEY CLUSTERED ([id] ASC),
);
GO

CREATE PROCEDURE [dbo].[insert_order]
  @order NVARCHAR(MAX)
AS
BEGIN
  declare @total decimal(8, 2)
  DECLARE @order_id UNIQUEIDENTIFIER

  /* ORDER ID */
  select @order_id = id
  FROM OPENJSON(@order) WITH ([id] uniqueidentifier '$.id')

  /* ORDER TOTAL */
  SELECT @total = SUM(op.quantity * p.price)
  FROM dbo.products p
    JOIN OPENJSON(@order, '$.orderProducts') WITH (
      [product_id] UNIQUEIDENTIFIER '$.product.id',
      [quantity]   INT              '$.quantity'
    ) op ON p.id = op.product_id

  BEGIN TRY
    BEGIN TRANSACTION

    /* ORDER */
    INSERT INTO dbo.orders(id, customer_name, customer_address, customer_phone_number, total_price, order_date, [status])
    SELECT @order_id, customer_name, customer_address, customer_phone_number, @total, GETUTCDATE(), [status]
    FROM OPENJSON(@order) WITH (
      [customer_name]         NVARCHAR(50)      '$.customerName',
      [customer_address]      NVARCHAR(50)      '$.customerAddress',
      [customer_phone_number] NVARCHAR(50)      '$.customerPhoneNumber',
      [status]                NVARCHAR(50)      '$.status'
    )

    /* ORDER PRODUCTS */
    INSERT INTO dbo.order_products(id, order_id, product_id, quantity, price)
    SELECT op.id, @order_id, op.product_id, op.quantity, (p.price * op.quantity)
    FROM dbo.products p
      JOIN OPENJSON(@order, '$.orderProducts') WITH (
        [id]                    UNIQUEIDENTIFIER  '$.id',
        [product_id]            UNIQUEIDENTIFIER  '$.product.id',
        [quantity]              INT               '$.quantity'
      ) op ON p.id = op.product_id

    COMMIT TRANSACTION
  END TRY
  BEGIN CATCH
    DECLARE @ErrorMessage NVARCHAR(4000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    SELECT @ErrorMessage = ERROR_MESSAGE(),
           @ErrorSeverity = ERROR_SEVERITY(),
           @ErrorState = ERROR_STATE();

    rollback transaction
    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
  END CATCH

  SELECT
    o.id                      AS [Id],
    o.customer_name           AS [CustomerName],
    o.customer_address        AS [CustomerAddress],
    o.customer_phone_number   AS [CustomerPhoneNumber],
    o.total_price             AS [TotalPrice],
    o.order_date              AS [OrderDate],
    o.[status]                AS [Status],
    OrderProducts.id          AS [Id],
    OrderProducts.quantity    AS [Quantity],
    OrderProducts.price       AS [Price],
    JSON_QUERY(
      (SELECT
        Product.id                  AS [Id],
        Product.name                AS [Name],
        Product.price               AS [Price]
      FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    )                         AS [Product]
  FROM dbo.orders o
    JOIN dbo.order_products OrderProducts ON o.id = OrderProducts.order_id
    JOIN dbo.products Product ON OrderProducts.product_id = Product.id
  WHERE o.id = @order_id
  FOR JSON AUTO, WITHOUT_ARRAY_WRAPPER
END
GO

CREATE PROCEDURE [dbo].[get_orders]
AS
BEGIN
  SELECT
    o.id                      AS [Id],
    o.customer_name           AS [CustomerName],
    o.customer_address        AS [CustomerAddress],
    o.customer_phone_number   AS [CustomerPhoneNumber],
    o.total_price             AS [TotalPrice],
    o.order_date              AS [OrderDate],
    o.[status]                AS [Status],
    OrderProducts.id          AS [Id],
    OrderProducts.quantity    AS [Quantity],
    OrderProducts.price       AS [Price],
    JSON_QUERY(
      (SELECT
        Product.id                  AS [Id],
        Product.name                AS [Name],
        Product.price               AS [Price]
      FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    )                         AS [Product]
  FROM dbo.orders o
    JOIN dbo.order_products OrderProducts ON o.id = OrderProducts.order_id
    JOIN dbo.products Product ON OrderProducts.product_id = Product.id
  FOR JSON AUTO
END
GO

CREATE PROCEDURE [dbo].[get_order_by_id]
  @order_id UNIQUEIDENTIFIER
AS
BEGIN
  SELECT
    o.id                      AS [Id],
    o.customer_name           AS [CustomerName],
    o.customer_address        AS [CustomerAddress],
    o.customer_phone_number   AS [CustomerPhoneNumber],
    o.total_price             AS [TotalPrice],
    o.order_date              AS [OrderDate],
    o.[status]                AS [Status],
    OrderProducts.id          AS [Id],
    OrderProducts.quantity    AS [Quantity],
    OrderProducts.price       AS [Price],
    JSON_QUERY(
      (SELECT
        Product.id                  AS [Id],
        Product.name                AS [Name],
        Product.price               AS [Price]
      FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    )                         AS [Product]
  FROM dbo.orders o
    JOIN dbo.order_products OrderProducts ON o.id = OrderProducts.order_id
    JOIN dbo.products Product ON OrderProducts.product_id = Product.id
  WHERE o.id = @order_id
  FOR JSON AUTO, WITHOUT_ARRAY_WRAPPER
END
GO

INSERT INTO dbo.products([id], [name], [price])
VALUES
  ('b3c7f7a0-0f1a-4f1a-9f1a-0f1a0f1a0f1a', 'Coca-Cola', 1.00),
  ('b3c7f7a0-0f1a-4f1a-9f1a-0f1a0f1a0f1b', 'Fanta', 2.00),
  ('b3c7f7a0-0f1a-4f1a-9f1a-0f1a0f1a0f1c', 'Sushi', 3.00),
  ('b3c7f7a0-0f1a-4f1a-9f1a-0f1a0f1a0f1d', 'Pizza', 4.00),
  ('b3c7f7a0-0f1a-4f1a-9f1a-0f1a0f1a0f1e', 'Hamburguer', 5.00)

GO

-- EXEC restaurant.dbo.insert_order N'{
--   "id": "b3c7f7a0-0f1a-4f1a-9f1a-0f1a0f1a0f1a",
--   "customerName": "John Doe",
--   "customerAddress": "123 Main St",
--   "customerPhoneNumber": "555-555-5555",
--   "status": "Pending",
--   "orderProducts": [
--     {
--       "id": "b3c7f7a0-0f1a-4f1a-9f1a-0f1a0f1a0f1a",
--       "product": {
--         "id": "b3c7f7a0-0f1a-4f1a-9f1a-0f1a0f1a0f1e"
--       },
--       "quantity": 1
--     },
--     {
--       "id": "b3c7f7a0-0f1a-4f1a-9f1a-0f1a0f1a0f1b",
--       "product": {
--         "id": "b3c7f7a0-0f1a-4f1a-9f1a-0f1a0f1a0f1d"
--       },
--       "quantity": 1
--     },
--     {
--       "id": "b3c7f7a0-0f1a-4f1a-9f1a-0f1a0f1a0f1c",
--       "product": {
--         "id": "b3c7f7a0-0f1a-4f1a-9f1a-0f1a0f1a0f1a"
--       },
--       "quantity": 2
--     }
--   ]
-- }'
