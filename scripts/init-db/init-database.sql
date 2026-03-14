-- ============================================
-- SisApi Database Initialization Script
-- SQL Server 2022 Express Edition
-- ============================================
-- This script runs automatically when the SQL Server container starts
-- It creates the database if it doesn't exist
-- ============================================

-- Create the database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'sisapi')
BEGIN
    CREATE DATABASE sisapi;
    PRINT 'Database sisapi created successfully.';
END
ELSE
BEGIN
    PRINT 'Database sisapi already exists.';
END
GO

-- Switch to the sisapi database
USE sisapi;
GO

-- Create a login for the application (optional, using sa in dev)
-- In production, create a specific user with limited permissions
-- Example:
-- CREATE LOGIN sisapi_user WITH PASSWORD = 'YourAppUserPassword123!';
-- CREATE USER sisapi_user FOR LOGIN sisapi_user;
-- ALTER ROLE db_owner ADD MEMBER sisapi_user;

PRINT 'Database initialization completed.';
GO
