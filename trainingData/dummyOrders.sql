USE ClipperCoffeeCorner;
GO

-- Insert guest-only dummy orders where:
--  - OrderItems are staged first, TotalAmount = SUM(UnitPrice * Quantity)
--  - Order is inserted AFTER TotalAmount is known (IdempotencyKey uses default)
--  - CompletedAt is calculated from TotalAmount
--  - OrderId is assigned by IDENTITY (incrementing)
SET NOCOUNT ON;

DECLARE @NumOrders INT = 1000; -- adjust as needed
DECLARE @i INT = 1;

IF OBJECT_ID('tempdb..#NewItems') IS NOT NULL DROP TABLE #NewItems;
CREATE TABLE #NewItems (
    BatchId UNIQUEIDENTIFIER NOT NULL,
    CombinationId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL
);

BEGIN TRANSACTION;
BEGIN TRY
    WHILE @i <= @NumOrders
    BEGIN
        DECLARE @BatchId UNIQUEIDENTIFIER = NEWID();

        -- random placed date within last 7 days
        DECLARE @PlacedAt DATETIME2 = SYSUTCDATETIME();

        -- stage 1..4 random items for this batch
        DECLARE @numItems INT = (ABS(CHECKSUM(NEWID())) % 4) + 1;
        DECLARE @j INT = 1;
        WHILE @j <= @numItems
        BEGIN
            DECLARE @CombId INT;
            SELECT TOP 1 @CombId = CombinationId FROM Combination ORDER BY NEWID();

            DECLARE @Qty INT = (ABS(CHECKSUM(NEWID())) % 3) + 1;
            DECLARE @UnitPrice DECIMAL(10,2) = ISNULL((SELECT Price FROM Combination WHERE CombinationId = @CombId), 0.00);

            INSERT INTO #NewItems (BatchId, CombinationId, Quantity, UnitPrice)
            VALUES (@BatchId, @CombId, @Qty, @UnitPrice);

            SET @j = @j + 1;
        END

        -- compute TotalAmount from staged items (LineTotal = UnitPrice * Quantity)
        DECLARE @TotalAmount DECIMAL(10,2) = ISNULL((SELECT SUM(UnitPrice * Quantity) FROM #NewItems WHERE BatchId = @BatchId), 0.00);

        -- compute CompletedAt after TotalAmount is known
        -- original formula: minutes = max(3, CEILING(TotalAmount * 3) + jitter(0..5))
        -- changed: divide the computed minutes by 20 (rounded up) and enforce a minimum of 3 minutes
        DECLARE @rndExtra INT = ABS(CHECKSUM(NEWID())) % 6;
        DECLARE @RawMinutes INT = CASE WHEN @TotalAmount <= 0 THEN 3 ELSE CEILING(@TotalAmount * 3.0) + @rndExtra END;
        DECLARE @MinutesToAdd INT = CASE
                                        WHEN CEILING(@RawMinutes / 20.0) < 3 THEN 3
                                        ELSE CEILING(@RawMinutes / 20.0)
                                    END;
        DECLARE @CompletedAt DATETIME2 = DATEADD(MINUTE, @MinutesToAdd, @PlacedAt);

        -- insert the Order AFTER total is computed; UserId = NULL (guest); let IdempotencyKey default
        INSERT INTO [Order] (UserId, Status, PlacedAt, CompletedAt, TotalAmount)
        VALUES (NULL, 'Completed', @PlacedAt, @CompletedAt, @TotalAmount);
        DECLARE @OrderId INT = SCOPE_IDENTITY();

        -- persist the staged OrderItem rows with the newly created OrderId
        INSERT INTO OrderItem (OrderId, CombinationId, Quantity, UnitPrice)
        SELECT @OrderId, CombinationId, Quantity, UnitPrice
        FROM #NewItems
        WHERE BatchId = @BatchId;

        -- cleanup staged rows for this batch
        DELETE FROM #NewItems WHERE BatchId = @BatchId;

        SET @i = @i + 1;
    END

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    DECLARE @errMsg NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR('Guest-only dummy order seed failed: %s', 16, 1, @errMsg);
END CATCH;

IF OBJECT_ID('tempdb..#NewItems') IS NOT NULL DROP TABLE #NewItems;
GO