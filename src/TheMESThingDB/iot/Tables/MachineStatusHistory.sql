
GO
CREATE INDEX IX_MachineStatusHistory_MachineId_EventDateUtc ON iot.MachineStatusHistory(MachineId, EventDateUtc, StartedAtUtc DESC) INCLUDE (MachineStateId);
GO
CREATE INDEX IX_MachineStatusHistory_TenantId_EventDateUtc ON iot.MachineStatusHistory(TenantId, EventDateUtc) INCLUDE (MachineId, MachineStateId);
GO
CREATE INDEX IX_MachineStatusHistory_ProductionOrderId ON iot.MachineStatusHistory(ProductionOrderId) WHERE ProductionOrderId IS NOT NULL;