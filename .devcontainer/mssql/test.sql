IF EXISTS(SELECT 1 FROM sys.databases WHERE name = 'restaurant')
BEGIN
  USE restaurant;
END
GO

CREATE PROCEDURE [dbo].[test_validate_order_products]
AS
BEGIN
  DECLARE @json NVARCHAR(MAX)
  SET @json = (
    SELECT op.id AS Id, op.price AS Price, op.quantity AS Quantity
    FROM order_products op
        LEFT JOIN orders o ON op.order_id = o.id
    WHERE o.id IS NULL
    FOR JSON AUTO
  )

  SELECT IIF(@json IS NULL, '[]', @json)
END