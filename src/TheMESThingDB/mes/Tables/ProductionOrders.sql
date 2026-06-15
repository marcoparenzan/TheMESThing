
GO
CREATE INDEX IX_ProductionOrders_WorkOrderId ON mes.ProductionOrders(WorkOrderId);
GO
CREATE INDEX IX_ProductionOrders_MachineId_Status ON mes.ProductionOrders(MachineId, Status);
GO
CREATE INDEX IX_ProductionOrders_PlannedStartAtUtc ON mes.ProductionOrders(PlannedStartAtUtc);