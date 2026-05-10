-- Migration: Seed sample orders for existing users with multiple quantities for men and women, across multiple categories

-- Assumptions:
-- - Users already exist in [UserService].[dbo].[Users]
-- - Products and categories already exist in [ProductService].[dbo].[Products] and [ProductService].[dbo].[Categories]
-- - Orders, OrderItems tables exist in current DB

-- This script should be run in the OrderService database

-- Example: Insert 2 orders per user, each with 2 items (one from Men, one from Women category)

DECLARE @UserId UNIQUEIDENTIFIER, @OrderId UNIQUEIDENTIFIER, @OrderItemId UNIQUEIDENTIFIER, @ProductId UNIQUEIDENTIFIER, @ProductName NVARCHAR(255), @Price DECIMAL(18,2), @Discount DECIMAL(18,2), @CategoryId INT;

DECLARE user_cursor CURSOR FOR
SELECT UserId FROM [UserService].[dbo].[Users];

OPEN user_cursor;
FETCH NEXT FROM user_cursor INTO @UserId;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- First order (Men)
    SET @OrderId = NEWID();
    INSERT INTO Orders (OrderId, UserId, Status, TotalAmount, PaymentMethod, ShippingAddress, BillingAddress, CreatedAt, Discount)
    VALUES (@OrderId, @UserId, 'Complete', 0, 'COD', 'Sample Address', 'Sample Address', GETDATE(), 0);

    -- Add 2 items from Men categories
    DECLARE men_cursor CURSOR FOR
    SELECT TOP 2 p.ProductId, p.Name, p.Price, p.Discount FROM [ProductService].[dbo].[Products] p
    INNER JOIN [ProductService].[dbo].[Categories] c ON p.CategoryId = c.CategoryId
    WHERE c.Gender = 'Men';
    OPEN men_cursor;
    FETCH NEXT FROM men_cursor INTO @ProductId, @ProductName, @Price, @Discount;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @OrderItemId = NEWID();
        INSERT INTO OrderItems (OrderItemId, OrderId, ProductId, ProductName, Quantity, Price, Discount)
        VALUES (@OrderItemId, @OrderId, @ProductId, @ProductName, 2, @Price, @Discount);
        FETCH NEXT FROM men_cursor INTO @ProductId, @ProductName, @Price, @Discount;
    END
    CLOSE men_cursor;
    DEALLOCATE men_cursor;

    -- Second order (Women)
    SET @OrderId = NEWID();
    INSERT INTO Orders (OrderId, UserId, Status, TotalAmount, PaymentMethod, ShippingAddress, BillingAddress, CreatedAt, Discount)
    VALUES (@OrderId, @UserId, 'Complete', 0, 'COD', 'Sample Address', 'Sample Address', GETDATE(), 0);

    -- Add 2 items from Women categories
    DECLARE women_cursor CURSOR FOR
    SELECT TOP 2 p.ProductId, p.Name, p.Price, p.Discount FROM [ProductService].[dbo].[Products] p
    INNER JOIN [ProductService].[dbo].[Categories] c ON p.CategoryId = c.CategoryId
    WHERE c.Gender = 'Women';
    OPEN women_cursor;
    FETCH NEXT FROM women_cursor INTO @ProductId, @ProductName, @Price, @Discount;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @OrderItemId = NEWID();
        INSERT INTO OrderItems (OrderItemId, OrderId, ProductId, ProductName, Quantity, Price, Discount)
        VALUES (@OrderItemId, @OrderId, @ProductId, @ProductName, 3, @Price, @Discount);
        FETCH NEXT FROM women_cursor INTO @ProductId, @ProductName, @Price, @Discount;
    END
    CLOSE women_cursor;
    DEALLOCATE women_cursor;

    FETCH NEXT FROM user_cursor INTO @UserId;
END

CLOSE user_cursor;
DEALLOCATE user_cursor;

-- Optionally, update TotalAmount for each order
UPDATE o
SET o.TotalAmount = (
    SELECT SUM((oi.Price - oi.Discount) * oi.Quantity)
    FROM OrderItems oi WHERE oi.OrderId = o.OrderId
)
FROM Orders o;

-- End of migration
