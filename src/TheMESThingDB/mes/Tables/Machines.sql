
GO
CREATE INDEX IX_Machines_TenantId_DepartmentId ON mes.Machines(TenantId, DepartmentId) INCLUDE (MachineCode, MachineName, CurrentStatus);
GO
CREATE INDEX IX_Machines_IoTDeviceId ON mes.Machines(IoTDeviceId) WHERE IoTDeviceId IS NOT NULL;