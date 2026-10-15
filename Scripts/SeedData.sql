SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @CategoryId int;
SELECT @CategoryId = Id
FROM dbo.Categories
WHERE Name = N'Papeleria';

IF @CategoryId IS NULL
BEGIN
    INSERT INTO dbo.Categories (Name)
    VALUES (N'Papeleria');

    SET @CategoryId = CONVERT(int, SCOPE_IDENTITY());
END;

DECLARE @NotebookId int;
SELECT @NotebookId = Id
FROM dbo.Productos
WHERE Name = N'Cuaderno A5' AND CategoryId = @CategoryId;

IF @NotebookId IS NULL
BEGIN
    INSERT INTO dbo.Productos (Name, Price, Description, Stock, CreatedAt, CategoryId)
    VALUES (N'Cuaderno A5', 4.50, N'Cuaderno de 80 hojas', 40, '2026-10-14T09:00:00', @CategoryId);

    SET @NotebookId = CONVERT(int, SCOPE_IDENTITY());
END;

DECLARE @PenId int;
SELECT @PenId = Id
FROM dbo.Productos
WHERE Name = N'Boligrafo azul' AND CategoryId = @CategoryId;

IF @PenId IS NULL
BEGIN
    INSERT INTO dbo.Productos (Name, Price, Description, Stock, CreatedAt, CategoryId)
    VALUES (N'Boligrafo azul', 1.25, N'Boligrafo de tinta azul', 100, '2026-10-14T09:00:00', @CategoryId);

    SET @PenId = CONVERT(int, SCOPE_IDENTITY());
END;

DECLARE @NotebookOrderId int;
SELECT @NotebookOrderId = Id
FROM dbo.Orders
WHERE ProductId = @NotebookId AND OrderDate = '2026-10-14T10:00:00';

IF @NotebookOrderId IS NULL
BEGIN
    INSERT INTO dbo.Orders (ProductId, Quantity, OrderDate)
    VALUES (@NotebookId, 2, '2026-10-14T10:00:00');

    SET @NotebookOrderId = CONVERT(int, SCOPE_IDENTITY());
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.OrderDetails
    WHERE OrderId = @NotebookOrderId AND ProductId = @NotebookId
)
BEGIN
    INSERT INTO dbo.OrderDetails (OrderId, ProductId, Quantity)
    VALUES (@NotebookOrderId, @NotebookId, 2);
END;

DECLARE @PenOrderId int;
SELECT @PenOrderId = Id
FROM dbo.Orders
WHERE ProductId = @PenId AND OrderDate = '2026-10-14T10:05:00';

IF @PenOrderId IS NULL
BEGIN
    INSERT INTO dbo.Orders (ProductId, Quantity, OrderDate)
    VALUES (@PenId, 5, '2026-10-14T10:05:00');

    SET @PenOrderId = CONVERT(int, SCOPE_IDENTITY());
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.OrderDetails
    WHERE OrderId = @PenOrderId AND ProductId = @PenId
)
BEGIN
    INSERT INTO dbo.OrderDetails (OrderId, ProductId, Quantity)
    VALUES (@PenOrderId, @PenId, 5);
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.InventoryLogs
    WHERE ProductId = @NotebookId AND LogDate = '2026-10-14T09:05:00'
)
BEGIN
    INSERT INTO dbo.InventoryLogs (ProductId, Quantity, LogDate)
    VALUES (@NotebookId, 40, '2026-10-14T09:05:00');
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.InventoryLogs
    WHERE ProductId = @PenId AND LogDate = '2026-10-14T09:05:00'
)
BEGIN
    INSERT INTO dbo.InventoryLogs (ProductId, Quantity, LogDate)
    VALUES (@PenId, 100, '2026-10-14T09:05:00');
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Logs
    WHERE Timestamp = '2026-10-14T09:10:00' AND Message = N'Datos de ejemplo cargados'
)
BEGIN
    INSERT INTO dbo.Logs (Timestamp, Level, Message)
    VALUES ('2026-10-14T09:10:00', N'Info', N'Datos de ejemplo cargados');
END;

COMMIT TRANSACTION;