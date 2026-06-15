-- =============================================================================
-- NeoMES Platform - Azure SQL Database DDL Script
-- Version: 1.0.0
-- Target: Azure SQL Database (General Purpose, SQL Server 2022 compatibility)
-- NOTE: Review partition schemes, filegroups, and retention policies
--       before deploying to production. Adjust collation if needed.
-- =============================================================================

-- =============================================================================
-- DATABASE CONFIGURATION (run as admin on target database)
-- =============================================================================
-- ALTER DATABASE NeoMES SET COMPATIBILITY_LEVEL = 160;
-- ALTER DATABASE NeoMES COLLATE SQL_Latin1_General_CP1_CI_AS;

-- =============================================================================
-- SCHEMA DEFINITIONS
-- =============================================================================
CREATE SCHEMA mes;