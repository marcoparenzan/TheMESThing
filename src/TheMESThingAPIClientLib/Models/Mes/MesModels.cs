namespace TheMESThingAPIClientLib.Models.Mes;

public record Customer(
    Guid CustomerId,
    Guid TenantId,
    string CustomerCode,
    string CustomerName,
    string? ContactEmail,
    string? ContactPhone,
    string? Address,
    string? Country,
    string? ExternalMicrosoft365Id,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    Guid? CreatedByUserId,
    Guid? UpdatedByUserId);

public record Department(
    Guid DepartmentId,
    Guid TenantId,
    string DepartmentCode,
    string DepartmentName,
    Guid? ParentDepartmentId,
    Guid? ManagerUserId,
    string? CostCenter,
    string? ExternalMicrosoft365Id,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public record ProductionLine(
    Guid ProductionLineId,
    Guid TenantId,
    Guid DepartmentId,
    string LineCode,
    string LineName,
    string LineType,
    double? NominalCapacity,
    string? CapacityUnit,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public record Machine(
    Guid MachineId,
    Guid TenantId,
    Guid DepartmentId,
    Guid? ProductionLineId,
    string MachineCode,
    string MachineName,
    string MachineType,
    string? Manufacturer,
    string? Model,
    string? SerialNumber,
    DateOnly? InstallationDate,
    double? NominalCycleTimeSeconds,
    double? NominalCapacityPerHour,
    string? IoTDeviceId,
    string? IoTHubConnectionState,
    string CurrentStatus,
    DateTime? LastStatusChangedAtUtc,
    string? ExternalMicrosoft365Id,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public record Skill(
    Guid SkillId,
    Guid TenantId,
    string SkillCode,
    string SkillName,
    string? Category,
    string? Description,
    bool IsActive,
    DateTime CreatedAtUtc);

public record MachineSkill(
    Guid MachineSkillId,
    Guid MachineId,
    Guid SkillId,
    bool IsRequired);

public record Operator(
    Guid OperatorId,
    Guid TenantId,
    Guid? AppUserId,
    Guid? DepartmentId,
    string OperatorCode,
    string FirstName,
    string LastName,
    string? BadgeNumber,
    string? ExternalMicrosoft365Id,
    bool IsActive,
    DateOnly? HireDate,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public record OperatorSkill(
    Guid OperatorSkillId,
    Guid OperatorId,
    Guid SkillId,
    byte ProficiencyLevel,
    DateTime? CertifiedAtUtc,
    DateTime? ExpiresAtUtc);

public record Shift(
    Guid ShiftId,
    Guid TenantId,
    string ShiftCode,
    string ShiftName,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int BreakMinutes,
    bool IsNightShift,
    bool IsActive,
    DateTime CreatedAtUtc);

public record Product(
    Guid ProductId,
    Guid TenantId,
    string ProductCode,
    string ProductName,
    string UnitOfMeasure,
    double? CycleTimeSeconds,
    double? SetupTimeSeconds,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public record WorkOrder(
    Guid WorkOrderId,
    Guid TenantId,
    Guid CustomerId,
    string WorkOrderNumber,
    string? Description,
    string Status,
    byte Priority,
    DateOnly? DueDate,
    DateOnly? PlannedStartDate,
    DateOnly? ActualStartDate,
    DateOnly? ActualEndDate,
    double TotalQuantity,
    double CompletedQuantity,
    double RejectedQuantity,
    string? ExternalMicrosoft365Id,
    string? TeamsChannelId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    Guid? CreatedByUserId,
    Guid? UpdatedByUserId);

public record ProductionOrder(
    Guid ProductionOrderId,
    Guid TenantId,
    Guid WorkOrderId,
    Guid ProductId,
    Guid? MachineId,
    Guid? ProductionLineId,
    int OperationSequence,
    string OrderNumber,
    string Status,
    double PlannedQuantity,
    double GoodQuantity,
    double ScrapQuantity,
    double ReworkQuantity,
    DateTime? PlannedStartAtUtc,
    DateTime? PlannedEndAtUtc,
    DateTime? ActualStartAtUtc,
    DateTime? ActualEndAtUtc,
    double? PlannedCycleTimeSeconds,
    double? ActualCycleTimeSeconds,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
