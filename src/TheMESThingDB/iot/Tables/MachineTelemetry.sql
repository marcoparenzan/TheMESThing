
GO
CREATE INDEX IX_MachineTelemetry_MachineId_MetricName_RecordedAtUtc ON iot.MachineTelemetry(MachineId, MetricName, RecordedAtUtc DESC);