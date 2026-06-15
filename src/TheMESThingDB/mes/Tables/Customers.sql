-- =============================================================================
-- MES SCHEMA â€” CORE BUSINESS ENTITIES
-- =============================================================================


GO
CREATE INDEX IX_Customers_TenantId ON mes.Customers(TenantId) INCLUDE (CustomerCode, CustomerName, IsActive);