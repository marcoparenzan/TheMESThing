
GO
CREATE INDEX IX_AppUsers_TenantId_IsActive ON security.AppUsers(TenantId, IsActive) INCLUDE (DisplayName, UserPrincipalName);