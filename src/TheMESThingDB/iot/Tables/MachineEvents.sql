
GO
CREATE INDEX IX_MachineEvents_MachineId_OccurredAtUtc ON iot.MachineEvents(MachineId, OccurredAtUtc DESC) INCLUDE (EventType, Severity);
GO
CREATE INDEX IX_MachineEvents_Severity_AcknowledgedAtUtc ON iot.MachineEvents(Severity, AcknowledgedAtUtc) WHERE AcknowledgedAtUtc IS NULL;