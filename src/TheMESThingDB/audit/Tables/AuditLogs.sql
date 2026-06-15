-- =============================================================================
-- AUDIT SCHEMA
-- =============================================================================


GO
CREATE INDEX IX_AuditLogs_TenantId_OccurredAtUtc ON audit.AuditLogs(TenantId, OccurredAtUtc DESC);
GO
CREATE INDEX IX_AuditLogs_EntityType_EntityId ON audit.AuditLogs(EntityType, EntityId);