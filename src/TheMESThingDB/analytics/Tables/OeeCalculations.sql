
GO
CREATE INDEX IX_OeeCalculations_MachineId_PeriodDate ON analytics.OeeCalculations(MachineId, PeriodDate DESC) INCLUDE (OeeValue, Availability, Performance, Quality);
GO
CREATE INDEX IX_OeeCalculations_TenantId_PeriodDate_PeriodType ON analytics.OeeCalculations(TenantId, PeriodDate, PeriodType);