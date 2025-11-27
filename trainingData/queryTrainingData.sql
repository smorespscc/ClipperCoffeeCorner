-- Query TrainingData: return only OrderString and duration (seconds) between PlacedAt and CompletedAt.
USE ClipperCoffeeCorner;
GO

SELECT
    td.OrderString,
    DATEDIFF(SECOND, td.PlacedAt, td.CompletedAt) AS DurationSeconds
FROM dbo.TrainingData td
ORDER BY td.PlacedAt DESC;
GO