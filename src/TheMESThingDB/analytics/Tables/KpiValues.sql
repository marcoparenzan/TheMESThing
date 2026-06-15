
GO
CREATE INDEX IX_KpiValues_KpiDefinitionId_PeriodDate ON analytics.KpiValues(KpiDefinitionId, PeriodDate DESC);
GO
CREATE INDEX IX_KpiValues_MachineId_PeriodDate ON analytics.KpiValues(MachineId, PeriodDate DESC) WHERE MachineId IS NOT NULL;