-- Drop database if it exists (forces disconnection of active sessions).
IF DB_ID(N'ClipperCoffeeCorner') IS NOT NULL
BEGIN
    -- Force other connections to close, rolling back transactions immediately.
    ALTER DATABASE [ClipperCoffeeCorner] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [ClipperCoffeeCorner];
    PRINT N'Database [ClipperCoffeeCorner] dropped.';
END
ELSE
BEGIN
    PRINT N'Database [ClipperCoffeeCorner] does not exist.';
END
GO
-- Create database if it does not already exist.
IF DB_ID(N'ClipperCoffeeCorner') IS NULL
BEGIN
    CREATE DATABASE [ClipperCoffeeCorner];
    PRINT N'Database [ClipperCoffeeCorner] created.';
END
ELSE
BEGIN
    PRINT N'Database [ClipperCoffeeCorner] already exists.';
END
GO

-- Optional: set simple recovery (safe for development/testing)
ALTER DATABASE [ClipperCoffeeCorner] SET RECOVERY SIMPLE WITH NO_WAIT;
GO

-- Switch to the new database
USE [ClipperCoffeeCorner];
GO