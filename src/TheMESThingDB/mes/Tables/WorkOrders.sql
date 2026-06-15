
GO
CREATE INDEX IX_WorkOrders_TenantId_Status ON mes.WorkOrders(TenantId, Status) INCLUDE (WorkOrderNumber, CustomerId, DueDate, Priority);
GO
CREATE INDEX IX_WorkOrders_CustomerId ON mes.WorkOrders(CustomerId);