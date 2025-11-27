USE ClipperCoffeeCorner;
GO

IF OBJECT_ID('dbo.TrainingData', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.TrainingData;
END
GO

CREATE TABLE dbo.TrainingData (
    PlacedAt      DATETIME2       NOT NULL,
    CompletedAt   DATETIME2       NOT NULL,
    OrderId       INT             NOT NULL,
    OrderString   NVARCHAR(400)   NULL,
    CONSTRAINT PK_TrainingData_OrderId PRIMARY KEY (OrderId),
    CONSTRAINT FK_TrainingData_Order FOREIGN KEY (OrderId)
        REFERENCES [Order](OrderId)
);
GO

SET NOCOUNT ON;

-- Populate/refresh TrainingData based on existing orders.
-- PlacedAt = Order.PlacedAt, CompletedAt = Order.CompletedAt.
-- OrderString = concatenated Combination.MenuItemId values for the Order's OrderItems (ordered by OrderItemId).
-- Only processes orders that have a non-NULL CompletedAt.
BEGIN TRANSACTION;
BEGIN TRY

    WITH OrderStrings AS (
        SELECT
            oi.OrderId,
            STRING_AGG(CONVERT(NVARCHAR(50), c.MenuItemId), ',') WITHIN GROUP (ORDER BY oi.OrderItemId) AS OrderString
        FROM OrderItem oi
        JOIN Combination c
            ON c.CombinationId = oi.CombinationId
        GROUP BY oi.OrderId
    ),
    SourceData AS (
        SELECT
            o.OrderId,
            o.PlacedAt,
            o.CompletedAt,
            ISNULL(os.OrderString, N'') AS OrderString
        FROM [Order] o
        LEFT JOIN OrderStrings os ON os.OrderId = o.OrderId
        WHERE o.CompletedAt IS NOT NULL
    )

    MERGE INTO dbo.TrainingData AS target
    USING SourceData AS src
        ON target.OrderId = src.OrderId
    WHEN MATCHED AND (target.PlacedAt <> src.PlacedAt OR target.CompletedAt <> src.CompletedAt OR ISNULL(target.OrderString, N'') <> src.OrderString)
        THEN UPDATE SET
            PlacedAt = src.PlacedAt,
            CompletedAt = src.CompletedAt,
            OrderString = src.OrderString
    WHEN NOT MATCHED BY TARGET
        THEN INSERT (PlacedAt, CompletedAt, OrderId, OrderString)
             VALUES (src.PlacedAt, src.CompletedAt, src.OrderId, src.OrderString)
    ;
    -- (No DELETE for rows missing from source — keep training records unless you want them removed.)

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    DECLARE @err NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR('Failed to populate TrainingData: %s', 16, 1, @err);
END CATCH;
GO

