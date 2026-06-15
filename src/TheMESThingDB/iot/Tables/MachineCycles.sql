
GO
CREATE INDEX IX_MachineCycles_ProductionOrderId_CycleDateUtc ON iot.MachineCycles(ProductionOrderId, CycleDateUtc);
GO
CREATE INDEX IX_MachineCycles_MachineId_CycleDateUtc ON iot.MachineCycles(MachineId, CycleDateUtc);