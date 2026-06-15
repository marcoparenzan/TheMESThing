-- =============================================================================
-- NeoMES Platform - Azure SQL Database DDL Script
-- Version: 1.0.0
-- Target: Azure SQL Database (General Purpose, SQL Server 2022 compatibility)
-- NOTE: Review partition schemes, filegroups, and retention policies
--       before deploying to production. Adjust collation if needed.
-- =============================================================================

-- =============================================================================
-- DATABASE CONFIGURATION (run as admin on target database)
-- =============================================================================
-- ALTER DATABASE NeoMES SET COMPATIBILITY_LEVEL = 160;
-- ALTER DATABASE NeoMES COLLATE SQL_Latin1_General_CP1_CI_AS;

-- =============================================================================
-- SCHEMA DEFINITIONS
-- =============================================================================
CREATE SCHEMA mes;
GO
CREATE SCHEMA iot;
GO
CREATE SCHEMA analytics;
GO
CREATE SCHEMA integration;
GO
CREATE SCHEMA security;
GO
CREATE SCHEMA audit;
GO

-- =============================================================================
-- SECURITY SCHEMA
-- =============================================================================

CREATE TABLE security.Tenants (
    TenantId         UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantCode       NVARCHAR(50)      NOT NULL,
    TenantName       NVARCHAR(200)     NOT NULL,
    EntraIdTenantId  NVARCHAR(100)     NULL,
    IsActive         BIT               NOT NULL DEFAULT 1,
    CreatedAtUtc     DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc     DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Tenants PRIMARY KEY (TenantId),
    CONSTRAINT UQ_Tenants_TenantCode UNIQUE (TenantCode)
);
GO

CREATE TABLE security.Roles (
    RoleId       UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId     UNIQUEIDENTIFIER  NOT NULL,
    RoleCode     NVARCHAR(100)     NOT NULL,
    RoleName     NVARCHAR(200)     NOT NULL,
    Description  NVARCHAR(500)     NULL,
    IsSystemRole BIT               NOT NULL DEFAULT 0,
    IsActive     BIT               NOT NULL DEFAULT 1,
    CreatedAtUtc DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Roles PRIMARY KEY (RoleId),
    CONSTRAINT FK_Roles_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT UQ_Roles_TenantCode UNIQUE (TenantId, RoleCode)
);
GO

CREATE TABLE security.Permissions (
    PermissionId  UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    PermissionCode NVARCHAR(100)    NOT NULL,
    Module        NVARCHAR(100)     NOT NULL,
    Action        NVARCHAR(100)     NOT NULL,
    Description   NVARCHAR(500)     NULL,
    CONSTRAINT PK_Permissions PRIMARY KEY (PermissionId),
    CONSTRAINT UQ_Permissions_Code UNIQUE (PermissionCode)
);
GO

CREATE TABLE security.RolePermissions (
    RolePermissionId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    RoleId           UNIQUEIDENTIFIER NOT NULL,
    PermissionId     UNIQUEIDENTIFIER NOT NULL,
    GrantedAtUtc     DATETIME2(7)     NOT NULL DEFAULT SYSUTCDATETIME(),
    GrantedByUserId  UNIQUEIDENTIFIER NULL,
    CONSTRAINT PK_RolePermissions PRIMARY KEY (RolePermissionId),
    CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (RoleId) REFERENCES security.Roles(RoleId),
    CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES security.Permissions(PermissionId),
    CONSTRAINT UQ_RolePermissions_RolePermission UNIQUE (RoleId, PermissionId)
);
GO

CREATE TABLE security.AppUsers (
    AppUserId              UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId               UNIQUEIDENTIFIER  NOT NULL,
    EntraIdObjectId        NVARCHAR(100)     NOT NULL,
    UserPrincipalName      NVARCHAR(300)     NOT NULL,
    DisplayName            NVARCHAR(300)     NOT NULL,
    Email                  NVARCHAR(300)     NULL,
    Department             NVARCHAR(200)     NULL,
    JobTitle               NVARCHAR(200)     NULL,
    IsActive               BIT               NOT NULL DEFAULT 1,
    LastLoginAtUtc         DATETIME2(7)      NULL,
    CreatedAtUtc           DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc           DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_AppUsers PRIMARY KEY (AppUserId),
    CONSTRAINT FK_AppUsers_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT UQ_AppUsers_EntraId UNIQUE (TenantId, EntraIdObjectId)
);
GO

CREATE TABLE security.UserRoles (
    UserRoleId   UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    AppUserId    UNIQUEIDENTIFIER  NOT NULL,
    RoleId       UNIQUEIDENTIFIER  NOT NULL,
    AssignedAtUtc DATETIME2(7)     NOT NULL DEFAULT SYSUTCDATETIME(),
    AssignedByUserId UNIQUEIDENTIFIER NULL,
    ExpiresAtUtc DATETIME2(7)      NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserRoleId),
    CONSTRAINT FK_UserRoles_AppUsers FOREIGN KEY (AppUserId) REFERENCES security.AppUsers(AppUserId),
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES security.Roles(RoleId),
    CONSTRAINT UQ_UserRoles_UserRole UNIQUE (AppUserId, RoleId)
);
GO

-- =============================================================================
-- MES SCHEMA — CORE BUSINESS ENTITIES
-- =============================================================================

CREATE TABLE mes.Customers (
    CustomerId                UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId                  UNIQUEIDENTIFIER  NOT NULL,
    CustomerCode              NVARCHAR(50)      NOT NULL,
    CustomerName              NVARCHAR(300)     NOT NULL,
    ContactEmail              NVARCHAR(300)     NULL,
    ContactPhone              NVARCHAR(50)      NULL,
    Address                   NVARCHAR(500)     NULL,
    Country                   NVARCHAR(100)     NULL,
    ExternalMicrosoft365Id    NVARCHAR(200)     NULL,
    IsActive                  BIT               NOT NULL DEFAULT 1,
    CreatedAtUtc              DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc              DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedByUserId           UNIQUEIDENTIFIER  NULL,
    UpdatedByUserId           UNIQUEIDENTIFIER  NULL,
    CONSTRAINT PK_Customers PRIMARY KEY (CustomerId),
    CONSTRAINT FK_Customers_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT UQ_Customers_Code UNIQUE (TenantId, CustomerCode)
);
GO

CREATE TABLE mes.Departments (
    DepartmentId           UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId               UNIQUEIDENTIFIER  NOT NULL,
    DepartmentCode         NVARCHAR(50)      NOT NULL,
    DepartmentName         NVARCHAR(200)     NOT NULL,
    ParentDepartmentId     UNIQUEIDENTIFIER  NULL,
    ManagerUserId          UNIQUEIDENTIFIER  NULL,
    CostCenter             NVARCHAR(50)      NULL,
    ExternalMicrosoft365Id NVARCHAR(200)     NULL,
    IsActive               BIT               NOT NULL DEFAULT 1,
    CreatedAtUtc           DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc           DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Departments PRIMARY KEY (DepartmentId),
    CONSTRAINT FK_Departments_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT FK_Departments_Parent FOREIGN KEY (ParentDepartmentId) REFERENCES mes.Departments(DepartmentId),
    CONSTRAINT UQ_Departments_Code UNIQUE (TenantId, DepartmentCode)
);
GO

CREATE TABLE mes.ProductionLines (
    ProductionLineId   UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId           UNIQUEIDENTIFIER  NOT NULL,
    DepartmentId       UNIQUEIDENTIFIER  NOT NULL,
    LineCode           NVARCHAR(50)      NOT NULL,
    LineName           NVARCHAR(200)     NOT NULL,
    LineType           NVARCHAR(50)      NOT NULL DEFAULT 'Assembly',
    NominalCapacity    DECIMAL(18,4)     NULL,
    CapacityUnit       NVARCHAR(50)      NULL,
    IsActive           BIT               NOT NULL DEFAULT 1,
    CreatedAtUtc       DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc       DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ProductionLines PRIMARY KEY (ProductionLineId),
    CONSTRAINT FK_ProductionLines_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT FK_ProductionLines_Departments FOREIGN KEY (DepartmentId) REFERENCES mes.Departments(DepartmentId),
    CONSTRAINT UQ_ProductionLines_Code UNIQUE (TenantId, LineCode)
);
GO

CREATE TABLE mes.Machines (
    MachineId              UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId               UNIQUEIDENTIFIER  NOT NULL,
    DepartmentId           UNIQUEIDENTIFIER  NOT NULL,
    ProductionLineId       UNIQUEIDENTIFIER  NULL,
    MachineCode            NVARCHAR(50)      NOT NULL,
    MachineName            NVARCHAR(200)     NOT NULL,
    MachineType            NVARCHAR(100)     NOT NULL,
    Manufacturer           NVARCHAR(200)     NULL,
    Model                  NVARCHAR(200)     NULL,
    SerialNumber           NVARCHAR(100)     NULL,
    InstallationDate       DATE              NULL,
    NominalCycleTimeSeconds DECIMAL(18,4)   NULL,
    NominalCapacityPerHour DECIMAL(18,4)    NULL,
    IoTDeviceId            NVARCHAR(200)     NULL,
    IoTHubConnectionState  NVARCHAR(50)      NULL DEFAULT 'Unknown',
    CurrentStatus          NVARCHAR(50)      NOT NULL DEFAULT 'Unknown',
    LastStatusChangedAtUtc DATETIME2(7)      NULL,
    ExternalMicrosoft365Id NVARCHAR(200)     NULL,
    IsActive               BIT               NOT NULL DEFAULT 1,
    CreatedAtUtc           DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc           DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Machines PRIMARY KEY (MachineId),
    CONSTRAINT FK_Machines_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT FK_Machines_Departments FOREIGN KEY (DepartmentId) REFERENCES mes.Departments(DepartmentId),
    CONSTRAINT FK_Machines_ProductionLines FOREIGN KEY (ProductionLineId) REFERENCES mes.ProductionLines(ProductionLineId),
    CONSTRAINT UQ_Machines_Code UNIQUE (TenantId, MachineCode),
    CONSTRAINT CK_Machines_CurrentStatus CHECK (CurrentStatus IN ('Running','Stopped','Idle','Maintenance','Fault','Setup','Unknown'))
);
GO

CREATE TABLE mes.Skills (
    SkillId      UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId     UNIQUEIDENTIFIER  NOT NULL,
    SkillCode    NVARCHAR(50)      NOT NULL,
    SkillName    NVARCHAR(200)     NOT NULL,
    Category     NVARCHAR(100)     NULL,
    Description  NVARCHAR(500)     NULL,
    IsActive     BIT               NOT NULL DEFAULT 1,
    CreatedAtUtc DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Skills PRIMARY KEY (SkillId),
    CONSTRAINT FK_Skills_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT UQ_Skills_Code UNIQUE (TenantId, SkillCode)
);
GO

CREATE TABLE mes.Operators (
    OperatorId             UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId               UNIQUEIDENTIFIER  NOT NULL,
    AppUserId              UNIQUEIDENTIFIER  NULL,
    DepartmentId           UNIQUEIDENTIFIER  NULL,
    OperatorCode           NVARCHAR(50)      NOT NULL,
    FirstName              NVARCHAR(150)     NOT NULL,
    LastName               NVARCHAR(150)     NOT NULL,
    BadgeNumber            NVARCHAR(50)      NULL,
    ExternalMicrosoft365Id NVARCHAR(200)     NULL,
    IsActive               BIT               NOT NULL DEFAULT 1,
    HireDate               DATE              NULL,
    CreatedAtUtc           DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc           DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Operators PRIMARY KEY (OperatorId),
    CONSTRAINT FK_Operators_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT FK_Operators_AppUsers FOREIGN KEY (AppUserId) REFERENCES security.AppUsers(AppUserId),
    CONSTRAINT FK_Operators_Departments FOREIGN KEY (DepartmentId) REFERENCES mes.Departments(DepartmentId),
    CONSTRAINT UQ_Operators_Code UNIQUE (TenantId, OperatorCode)
);
GO

CREATE TABLE mes.OperatorSkills (
    OperatorSkillId  UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    OperatorId       UNIQUEIDENTIFIER  NOT NULL,
    SkillId          UNIQUEIDENTIFIER  NOT NULL,
    ProficiencyLevel TINYINT           NOT NULL DEFAULT 1,
    CertifiedAtUtc   DATETIME2(7)      NULL,
    ExpiresAtUtc     DATETIME2(7)      NULL,
    CONSTRAINT PK_OperatorSkills PRIMARY KEY (OperatorSkillId),
    CONSTRAINT FK_OperatorSkills_Operators FOREIGN KEY (OperatorId) REFERENCES mes.Operators(OperatorId),
    CONSTRAINT FK_OperatorSkills_Skills FOREIGN KEY (SkillId) REFERENCES mes.Skills(SkillId),
    CONSTRAINT UQ_OperatorSkills_OperatorSkill UNIQUE (OperatorId, SkillId),
    CONSTRAINT CK_OperatorSkills_Proficiency CHECK (ProficiencyLevel BETWEEN 1 AND 5)
);
GO

CREATE TABLE mes.MachineSkills (
    MachineSkillId UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    MachineId      UNIQUEIDENTIFIER  NOT NULL,
    SkillId        UNIQUEIDENTIFIER  NOT NULL,
    IsRequired     BIT               NOT NULL DEFAULT 1,
    CONSTRAINT PK_MachineSkills PRIMARY KEY (MachineSkillId),
    CONSTRAINT FK_MachineSkills_Machines FOREIGN KEY (MachineId) REFERENCES mes.Machines(MachineId),
    CONSTRAINT FK_MachineSkills_Skills FOREIGN KEY (SkillId) REFERENCES mes.Skills(SkillId),
    CONSTRAINT UQ_MachineSkills_MachineSkill UNIQUE (MachineId, SkillId)
);
GO

CREATE TABLE mes.Shifts (
    ShiftId       UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId      UNIQUEIDENTIFIER  NOT NULL,
    ShiftCode     NVARCHAR(50)      NOT NULL,
    ShiftName     NVARCHAR(200)     NOT NULL,
    StartTime     TIME(0)           NOT NULL,
    EndTime       TIME(0)           NOT NULL,
    BreakMinutes  INT               NOT NULL DEFAULT 0,
    IsNightShift  BIT               NOT NULL DEFAULT 0,
    IsActive      BIT               NOT NULL DEFAULT 1,
    CreatedAtUtc  DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Shifts PRIMARY KEY (ShiftId),
    CONSTRAINT FK_Shifts_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT UQ_Shifts_Code UNIQUE (TenantId, ShiftCode)
);
GO

CREATE TABLE mes.Products (
    ProductId     UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId      UNIQUEIDENTIFIER  NOT NULL,
    ProductCode   NVARCHAR(50)      NOT NULL,
    ProductName   NVARCHAR(300)     NOT NULL,
    UnitOfMeasure NVARCHAR(20)      NOT NULL DEFAULT 'PCS',
    CycleTimeSeconds DECIMAL(18,4)  NULL,
    SetupTimeSeconds DECIMAL(18,4)  NULL,
    IsActive      BIT               NOT NULL DEFAULT 1,
    CreatedAtUtc  DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc  DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Products PRIMARY KEY (ProductId),
    CONSTRAINT FK_Products_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT UQ_Products_Code UNIQUE (TenantId, ProductCode)
);
GO

CREATE TABLE mes.WorkOrders (
    WorkOrderId            UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId               UNIQUEIDENTIFIER  NOT NULL,
    CustomerId             UNIQUEIDENTIFIER  NOT NULL,
    WorkOrderNumber        NVARCHAR(50)      NOT NULL,
    Description            NVARCHAR(500)     NULL,
    Status                 NVARCHAR(50)      NOT NULL DEFAULT 'Draft',
    Priority               TINYINT           NOT NULL DEFAULT 3,
    DueDate                DATE              NULL,
    PlannedStartDate       DATE              NULL,
    ActualStartDate        DATE              NULL,
    ActualEndDate          DATE              NULL,
    TotalQuantity          DECIMAL(18,4)     NOT NULL DEFAULT 0,
    CompletedQuantity      DECIMAL(18,4)     NOT NULL DEFAULT 0,
    RejectedQuantity       DECIMAL(18,4)     NOT NULL DEFAULT 0,
    ExternalMicrosoft365Id NVARCHAR(200)     NULL,
    TeamsChannelId         NVARCHAR(200)     NULL,
    CreatedAtUtc           DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc           DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedByUserId        UNIQUEIDENTIFIER  NULL,
    UpdatedByUserId        UNIQUEIDENTIFIER  NULL,
    CONSTRAINT PK_WorkOrders PRIMARY KEY (WorkOrderId),
    CONSTRAINT FK_WorkOrders_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT FK_WorkOrders_Customers FOREIGN KEY (CustomerId) REFERENCES mes.Customers(CustomerId),
    CONSTRAINT UQ_WorkOrders_Number UNIQUE (TenantId, WorkOrderNumber),
    CONSTRAINT CK_WorkOrders_Status CHECK (Status IN ('Draft','Released','InProgress','Completed','Cancelled','OnHold')),
    CONSTRAINT CK_WorkOrders_Priority CHECK (Priority BETWEEN 1 AND 5),
    CONSTRAINT CK_WorkOrders_Quantities CHECK (CompletedQuantity >= 0 AND RejectedQuantity >= 0)
);
GO

CREATE TABLE mes.ProductionOrders (
    ProductionOrderId      UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId               UNIQUEIDENTIFIER  NOT NULL,
    WorkOrderId            UNIQUEIDENTIFIER  NOT NULL,
    ProductId              UNIQUEIDENTIFIER  NOT NULL,
    MachineId              UNIQUEIDENTIFIER  NULL,
    ProductionLineId       UNIQUEIDENTIFIER  NULL,
    OperationSequence      INT               NOT NULL DEFAULT 10,
    OrderNumber            NVARCHAR(50)      NOT NULL,
    Status                 NVARCHAR(50)      NOT NULL DEFAULT 'Planned',
    PlannedQuantity        DECIMAL(18,4)     NOT NULL,
    GoodQuantity           DECIMAL(18,4)     NOT NULL DEFAULT 0,
    ScrapQuantity          DECIMAL(18,4)     NOT NULL DEFAULT 0,
    ReworkQuantity         DECIMAL(18,4)     NOT NULL DEFAULT 0,
    PlannedStartAtUtc      DATETIME2(7)      NULL,
    PlannedEndAtUtc        DATETIME2(7)      NULL,
    ActualStartAtUtc       DATETIME2(7)      NULL,
    ActualEndAtUtc         DATETIME2(7)      NULL,
    PlannedCycleTimeSeconds DECIMAL(18,4)   NULL,
    ActualCycleTimeSeconds  DECIMAL(18,4)   NULL,
    CreatedAtUtc           DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc           DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ProductionOrders PRIMARY KEY (ProductionOrderId),
    CONSTRAINT FK_ProductionOrders_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT FK_ProductionOrders_WorkOrders FOREIGN KEY (WorkOrderId) REFERENCES mes.WorkOrders(WorkOrderId),
    CONSTRAINT FK_ProductionOrders_Products FOREIGN KEY (ProductId) REFERENCES mes.Products(ProductId),
    CONSTRAINT FK_ProductionOrders_Machines FOREIGN KEY (MachineId) REFERENCES mes.Machines(MachineId),
    CONSTRAINT FK_ProductionOrders_Lines FOREIGN KEY (ProductionLineId) REFERENCES mes.ProductionLines(ProductionLineId),
    CONSTRAINT UQ_ProductionOrders_Number UNIQUE (TenantId, OrderNumber),
    CONSTRAINT CK_ProductionOrders_Status CHECK (Status IN ('Planned','Released','InProgress','Completed','Cancelled','Suspended'))
);
GO

-- =============================================================================
-- IOT SCHEMA — TELEMETRY AND EVENTS
-- NOTE: MachineStatusHistory and MachineTelemetry are candidates for
--       table partitioning by EventDateUtc (monthly). Adjust partition
--       function and scheme before production deployment.
-- =============================================================================

CREATE TABLE iot.MachineStates (
    MachineStateId   INT           NOT NULL IDENTITY(1,1),
    StateCode        NVARCHAR(50)  NOT NULL,
    StateName        NVARCHAR(100) NOT NULL,
    StateCategory    NVARCHAR(50)  NOT NULL,
    IsProductiveTime BIT           NOT NULL DEFAULT 0,
    IsPlannedStop    BIT           NOT NULL DEFAULT 0,
    ColorHex         NVARCHAR(7)   NULL,
    SortOrder        INT           NOT NULL DEFAULT 0,
    CONSTRAINT PK_MachineStates PRIMARY KEY (MachineStateId),
    CONSTRAINT UQ_MachineStates_Code UNIQUE (StateCode),
    CONSTRAINT CK_MachineStates_Category CHECK (StateCategory IN ('Productive','Unplanned','Planned','External'))
);
GO

CREATE TABLE iot.MachineStatusHistory (
    MachineStatusHistoryId BIGINT           NOT NULL IDENTITY(1,1),
    TenantId               UNIQUEIDENTIFIER NOT NULL,
    MachineId              UNIQUEIDENTIFIER NOT NULL,
    MachineStateId         INT              NOT NULL,
    StartedAtUtc           DATETIME2(7)     NOT NULL,
    EndedAtUtc             DATETIME2(7)     NULL,
    DurationSeconds        AS (DATEDIFF(SECOND, StartedAtUtc, ISNULL(EndedAtUtc, SYSUTCDATETIME()))),
    ProductionOrderId      UNIQUEIDENTIFIER NULL,
    ShiftId                UNIQUEIDENTIFIER NULL,
    OperatorId             UNIQUEIDENTIFIER NULL,
    ReasonCode             NVARCHAR(100)    NULL,
    Notes                  NVARCHAR(500)    NULL,
    IoTMessageId           NVARCHAR(200)    NULL,
    CreatedAtUtc           DATETIME2(7)     NOT NULL DEFAULT SYSUTCDATETIME(),
    -- Partition column (TEMPLATE: enable when partition scheme is configured)
    EventDateUtc           DATE             NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATE),
    CONSTRAINT PK_MachineStatusHistory PRIMARY KEY (MachineStatusHistoryId, EventDateUtc),
    CONSTRAINT FK_MachineStatusHistory_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT FK_MachineStatusHistory_Machines FOREIGN KEY (MachineId) REFERENCES mes.Machines(MachineId),
    CONSTRAINT FK_MachineStatusHistory_MachineStates FOREIGN KEY (MachineStateId) REFERENCES iot.MachineStates(MachineStateId),
    CONSTRAINT FK_MachineStatusHistory_ProductionOrders FOREIGN KEY (ProductionOrderId) REFERENCES mes.ProductionOrders(ProductionOrderId),
    CONSTRAINT FK_MachineStatusHistory_Shifts FOREIGN KEY (ShiftId) REFERENCES mes.Shifts(ShiftId)
);
GO

CREATE TABLE iot.MachineEvents (
    MachineEventId    BIGINT           NOT NULL IDENTITY(1,1),
    TenantId          UNIQUEIDENTIFIER NOT NULL,
    MachineId         UNIQUEIDENTIFIER NOT NULL,
    EventType         NVARCHAR(100)    NOT NULL,
    EventCode         NVARCHAR(100)    NOT NULL,
    Severity          NVARCHAR(50)     NOT NULL DEFAULT 'Info',
    EventMessage      NVARCHAR(1000)   NULL,
    EventPayloadJson  NVARCHAR(MAX)    NULL,
    OccurredAtUtc     DATETIME2(7)     NOT NULL,
    AcknowledgedAtUtc DATETIME2(7)     NULL,
    AcknowledgedByUserId UNIQUEIDENTIFIER NULL,
    ProductionOrderId UNIQUEIDENTIFIER NULL,
    IoTMessageId      NVARCHAR(200)    NULL,
    EventDateUtc      DATE             NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATE),
    CONSTRAINT PK_MachineEvents PRIMARY KEY (MachineEventId, EventDateUtc),
    CONSTRAINT FK_MachineEvents_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT FK_MachineEvents_Machines FOREIGN KEY (MachineId) REFERENCES mes.Machines(MachineId),
    CONSTRAINT CK_MachineEvents_Severity CHECK (Severity IN ('Debug','Info','Warning','Error','Critical'))
);
GO

CREATE TABLE iot.MachineTelemetry (
    MachineTelemetryId  BIGINT           NOT NULL IDENTITY(1,1),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    MachineId           UNIQUEIDENTIFIER NOT NULL,
    MetricName          NVARCHAR(100)    NOT NULL,
    MetricValue         FLOAT            NOT NULL,
    MetricUnit          NVARCHAR(50)     NULL,
    RecordedAtUtc       DATETIME2(7)     NOT NULL,
    ProductionOrderId   UNIQUEIDENTIFIER NULL,
    IoTMessageId        NVARCHAR(200)    NULL,
    TelemetryDateUtc    DATE             NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATE),
    CONSTRAINT PK_MachineTelemetry PRIMARY KEY (MachineTelemetryId, TelemetryDateUtc),
    CONSTRAINT FK_MachineTelemetry_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT FK_MachineTelemetry_Machines FOREIGN KEY (MachineId) REFERENCES mes.Machines(MachineId)
);
GO

CREATE TABLE iot.MachineCycles (
    MachineCycleId        BIGINT           NOT NULL IDENTITY(1,1),
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    MachineId             UNIQUEIDENTIFIER NOT NULL,
    ProductionOrderId     UNIQUEIDENTIFIER NOT NULL,
    ShiftId               UNIQUEIDENTIFIER NULL,
    OperatorId            UNIQUEIDENTIFIER NULL,
    CycleStartAtUtc       DATETIME2(7)     NOT NULL,
    CycleEndAtUtc         DATETIME2(7)     NOT NULL,
    CycleDurationSeconds  AS (DATEDIFF(SECOND, CycleStartAtUtc, CycleEndAtUtc)) PERSISTED,
    GoodQuantity          DECIMAL(18,4)    NOT NULL DEFAULT 0,
    ScrapQuantity         DECIMAL(18,4)    NOT NULL DEFAULT 0,
    IsRejected            BIT              NOT NULL DEFAULT 0,
    ScrapReasonCode       NVARCHAR(100)    NULL,
    IoTMessageId          NVARCHAR(200)    NULL,
    CycleDateUtc          DATE             NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATE),
    CONSTRAINT PK_MachineCycles PRIMARY KEY (MachineCycleId, CycleDateUtc),
    CONSTRAINT FK_MachineCycles_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT FK_MachineCycles_Machines FOREIGN KEY (MachineId) REFERENCES mes.Machines(MachineId),
    CONSTRAINT FK_MachineCycles_ProductionOrders FOREIGN KEY (ProductionOrderId) REFERENCES mes.ProductionOrders(ProductionOrderId)
);
GO

-- =============================================================================
-- ANALYTICS SCHEMA — KPI, OEE, AND AGGREGATIONS
-- =============================================================================

CREATE TABLE analytics.KpiDefinitions (
    KpiDefinitionId  UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    KpiCode          NVARCHAR(50)      NOT NULL,
    KpiName          NVARCHAR(200)     NOT NULL,
    KpiGroup         NVARCHAR(100)     NOT NULL,
    IsoReference     NVARCHAR(50)      NULL,
    Formula          NVARCHAR(1000)    NULL,
    Unit             NVARCHAR(50)      NULL,
    TargetValue      DECIMAL(18,4)     NULL,
    WarningThreshold DECIMAL(18,4)     NULL,
    CriticalThreshold DECIMAL(18,4)   NULL,
    IsHigherBetter   BIT               NOT NULL DEFAULT 1,
    Description      NVARCHAR(1000)   NULL,
    CONSTRAINT PK_KpiDefinitions PRIMARY KEY (KpiDefinitionId),
    CONSTRAINT UQ_KpiDefinitions_Code UNIQUE (KpiCode)
);
GO

CREATE TABLE analytics.OeeCalculations (
    OeeCalculationId    BIGINT           NOT NULL IDENTITY(1,1),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    MachineId           UNIQUEIDENTIFIER NOT NULL,
    ShiftId             UNIQUEIDENTIFIER NULL,
    ProductionOrderId   UNIQUEIDENTIFIER NULL,
    PeriodDate          DATE             NOT NULL,
    PeriodType          NVARCHAR(20)     NOT NULL DEFAULT 'Shift',
    -- ISO 22400 OEE components
    PlannedProductionTimeSeconds  BIGINT       NOT NULL DEFAULT 0,
    OperatingTimeSeconds          BIGINT       NOT NULL DEFAULT 0,
    RunTimeSeconds                BIGINT       NOT NULL DEFAULT 0,
    PlannedQuantity               DECIMAL(18,4) NOT NULL DEFAULT 0,
    GoodQuantity                  DECIMAL(18,4) NOT NULL DEFAULT 0,
    TotalQuantity                 DECIMAL(18,4) NOT NULL DEFAULT 0,
    -- Calculated OEE factors (stored for performance)
    Availability      DECIMAL(10,6)    NULL,  -- OperatingTime / PlannedProductionTime
    Performance       DECIMAL(10,6)    NULL,  -- (TotalQty * IdealCycleTime) / OperatingTime
    Quality           DECIMAL(10,6)    NULL,  -- GoodQty / TotalQty
    OeeValue          DECIMAL(10,6)    NULL,  -- Availability * Performance * Quality
    -- Additional ISO 22400 KPIs
    MeanTimeBetweenFailureSeconds BIGINT  NULL,
    MeanTimeToRepairSeconds       BIGINT  NULL,
    ScheduledDowntimeSeconds      BIGINT  NOT NULL DEFAULT 0,
    UnscheduledDowntimeSeconds    BIGINT  NOT NULL DEFAULT 0,
    CalculatedAtUtc               DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_OeeCalculations PRIMARY KEY (OeeCalculationId),
    CONSTRAINT FK_OeeCalculations_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT FK_OeeCalculations_Machines FOREIGN KEY (MachineId) REFERENCES mes.Machines(MachineId),
    CONSTRAINT FK_OeeCalculations_ProductionOrders FOREIGN KEY (ProductionOrderId) REFERENCES mes.ProductionOrders(ProductionOrderId),
    CONSTRAINT CK_OeeCalculations_PeriodType CHECK (PeriodType IN ('Hour','Shift','Day','Week','Month','Year')),
    CONSTRAINT CK_OeeCalculations_OEE CHECK (OeeValue IS NULL OR (OeeValue >= 0 AND OeeValue <= 1))
);
GO

CREATE TABLE analytics.KpiValues (
    KpiValueId         BIGINT            NOT NULL IDENTITY(1,1),
    TenantId           UNIQUEIDENTIFIER  NOT NULL,
    KpiDefinitionId    UNIQUEIDENTIFIER  NOT NULL,
    MachineId          UNIQUEIDENTIFIER  NULL,
    DepartmentId       UNIQUEIDENTIFIER  NULL,
    WorkOrderId        UNIQUEIDENTIFIER  NULL,
    ShiftId            UNIQUEIDENTIFIER  NULL,
    PeriodDate         DATE              NOT NULL,
    PeriodType         NVARCHAR(20)      NOT NULL DEFAULT 'Day',
    KpiValue           DECIMAL(18,6)     NOT NULL,
    KpiTarget          DECIMAL(18,6)     NULL,
    VarianceFromTarget DECIMAL(18,6)     NULL,
    CalculatedAtUtc    DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_KpiValues PRIMARY KEY (KpiValueId),
    CONSTRAINT FK_KpiValues_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT FK_KpiValues_KpiDefinitions FOREIGN KEY (KpiDefinitionId) REFERENCES analytics.KpiDefinitions(KpiDefinitionId),
    CONSTRAINT FK_KpiValues_Machines FOREIGN KEY (MachineId) REFERENCES mes.Machines(MachineId),
    CONSTRAINT FK_KpiValues_Departments FOREIGN KEY (DepartmentId) REFERENCES mes.Departments(DepartmentId)
);
GO

CREATE TABLE analytics.ShiftReports (
    ShiftReportId       BIGINT            NOT NULL IDENTITY(1,1),
    TenantId            UNIQUEIDENTIFIER  NOT NULL,
    ShiftId             UNIQUEIDENTIFIER  NOT NULL,
    MachineId           UNIQUEIDENTIFIER  NOT NULL,
    OperatorId          UNIQUEIDENTIFIER  NULL,
    ShiftDate           DATE              NOT NULL,
    ShiftStartAtUtc     DATETIME2(7)      NOT NULL,
    ShiftEndAtUtc       DATETIME2(7)      NULL,
    PlannedProductionTimeSeconds BIGINT  NOT NULL DEFAULT 0,
    TotalRunTimeSeconds BIGINT            NOT NULL DEFAULT 0,
    TotalDownTimeSeconds BIGINT           NOT NULL DEFAULT 0,
    TotalIdleTimeSeconds BIGINT           NOT NULL DEFAULT 0,
    GoodQuantity        DECIMAL(18,4)     NOT NULL DEFAULT 0,
    ScrapQuantity       DECIMAL(18,4)     NOT NULL DEFAULT 0,
    TotalCycles         INT               NOT NULL DEFAULT 0,
    OeeValue            DECIMAL(10,6)     NULL,
    Notes               NVARCHAR(1000)    NULL,
    CreatedAtUtc        DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ShiftReports PRIMARY KEY (ShiftReportId),
    CONSTRAINT FK_ShiftReports_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT FK_ShiftReports_Shifts FOREIGN KEY (ShiftId) REFERENCES mes.Shifts(ShiftId),
    CONSTRAINT FK_ShiftReports_Machines FOREIGN KEY (MachineId) REFERENCES mes.Machines(MachineId)
);
GO

-- =============================================================================
-- INTEGRATION SCHEMA — MICROSOFT 365
-- =============================================================================

CREATE TABLE integration.M365SyncJobs (
    SyncJobId       UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId        UNIQUEIDENTIFIER  NOT NULL,
    EntityType      NVARCHAR(100)     NOT NULL,
    Status          NVARCHAR(50)      NOT NULL DEFAULT 'Pending',
    StartedAtUtc    DATETIME2(7)      NULL,
    CompletedAtUtc  DATETIME2(7)      NULL,
    RecordsProcessed INT              NOT NULL DEFAULT 0,
    RecordsFailed   INT               NOT NULL DEFAULT 0,
    ErrorDetails    NVARCHAR(MAX)     NULL,
    CreatedAtUtc    DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_M365SyncJobs PRIMARY KEY (SyncJobId),
    CONSTRAINT FK_M365SyncJobs_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT CK_M365SyncJobs_Status CHECK (Status IN ('Pending','Running','Completed','Failed','Cancelled'))
);
GO

CREATE TABLE integration.TeamsNotifications (
    NotificationId      UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER  NOT NULL,
    NotificationType    NVARCHAR(100)     NOT NULL,
    RecipientType       NVARCHAR(50)      NOT NULL DEFAULT 'Channel',
    RecipientId         NVARCHAR(300)     NOT NULL,
    Subject             NVARCHAR(500)     NOT NULL,
    BodyJson            NVARCHAR(MAX)     NULL,
    RelatedEntityType   NVARCHAR(100)     NULL,
    RelatedEntityId     NVARCHAR(200)     NULL,
    Status              NVARCHAR(50)      NOT NULL DEFAULT 'Pending',
    SentAtUtc           DATETIME2(7)      NULL,
    ErrorMessage        NVARCHAR(1000)    NULL,
    RetryCount          TINYINT           NOT NULL DEFAULT 0,
    CreatedAtUtc        DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_TeamsNotifications PRIMARY KEY (NotificationId),
    CONSTRAINT FK_TeamsNotifications_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT CK_TeamsNotifications_Status CHECK (Status IN ('Pending','Sent','Failed','Cancelled'))
);
GO

CREATE TABLE integration.CalendarEvents (
    CalendarEventId        UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId               UNIQUEIDENTIFIER  NOT NULL,
    M365EventId            NVARCHAR(300)     NULL,
    EventType              NVARCHAR(100)     NOT NULL,
    Title                  NVARCHAR(500)     NOT NULL,
    StartAtUtc             DATETIME2(7)      NOT NULL,
    EndAtUtc               DATETIME2(7)      NOT NULL,
    RelatedEntityType      NVARCHAR(100)     NULL,
    RelatedEntityId        NVARCHAR(200)     NULL,
    OrganizerUserId        UNIQUEIDENTIFIER  NULL,
    CalendarOwnerM365Id    NVARCHAR(300)     NULL,
    IsSynced               BIT               NOT NULL DEFAULT 0,
    SyncedAtUtc            DATETIME2(7)      NULL,
    CreatedAtUtc           DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc           DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_CalendarEvents PRIMARY KEY (CalendarEventId),
    CONSTRAINT FK_CalendarEvents_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId)
);
GO

-- =============================================================================
-- AUDIT SCHEMA
-- =============================================================================

CREATE TABLE audit.AuditLogs (
    AuditLogId      BIGINT            NOT NULL IDENTITY(1,1),
    TenantId        UNIQUEIDENTIFIER  NOT NULL,
    AppUserId       UNIQUEIDENTIFIER  NULL,
    UserPrincipalName NVARCHAR(300)   NULL,
    Action          NVARCHAR(100)     NOT NULL,
    EntityType      NVARCHAR(100)     NOT NULL,
    EntityId        NVARCHAR(200)     NULL,
    OldValuesJson   NVARCHAR(MAX)     NULL,
    NewValuesJson   NVARCHAR(MAX)     NULL,
    IpAddress       NVARCHAR(50)      NULL,
    UserAgent       NVARCHAR(500)     NULL,
    CorrelationId   NVARCHAR(100)     NULL,
    OccurredAtUtc   DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_AuditLogs PRIMARY KEY (AuditLogId)
);
GO

-- =============================================================================
-- INDEXES
-- =============================================================================

-- Customers
CREATE INDEX IX_Customers_TenantId ON mes.Customers(TenantId) INCLUDE (CustomerCode, CustomerName, IsActive);

-- WorkOrders
CREATE INDEX IX_WorkOrders_TenantId_Status ON mes.WorkOrders(TenantId, Status) INCLUDE (WorkOrderNumber, CustomerId, DueDate, Priority);
CREATE INDEX IX_WorkOrders_CustomerId ON mes.WorkOrders(CustomerId);
CREATE INDEX IX_WorkOrders_DueDate ON mes.WorkOrders(DueDate) WHERE Status NOT IN ('Completed','Cancelled');

-- ProductionOrders
CREATE INDEX IX_ProductionOrders_WorkOrderId ON mes.ProductionOrders(WorkOrderId);
CREATE INDEX IX_ProductionOrders_MachineId_Status ON mes.ProductionOrders(MachineId, Status);
CREATE INDEX IX_ProductionOrders_PlannedStartAtUtc ON mes.ProductionOrders(PlannedStartAtUtc);

-- Machines
CREATE INDEX IX_Machines_TenantId_DepartmentId ON mes.Machines(TenantId, DepartmentId) INCLUDE (MachineCode, MachineName, CurrentStatus);
CREATE INDEX IX_Machines_IoTDeviceId ON mes.Machines(IoTDeviceId) WHERE IoTDeviceId IS NOT NULL;

-- MachineStatusHistory (partitioned-friendly)
CREATE INDEX IX_MachineStatusHistory_MachineId_EventDateUtc ON iot.MachineStatusHistory(MachineId, EventDateUtc, StartedAtUtc DESC) INCLUDE (MachineStateId, DurationSeconds);
CREATE INDEX IX_MachineStatusHistory_TenantId_EventDateUtc ON iot.MachineStatusHistory(TenantId, EventDateUtc) INCLUDE (MachineId, MachineStateId);
CREATE INDEX IX_MachineStatusHistory_ProductionOrderId ON iot.MachineStatusHistory(ProductionOrderId) WHERE ProductionOrderId IS NOT NULL;

-- MachineEvents
CREATE INDEX IX_MachineEvents_MachineId_OccurredAtUtc ON iot.MachineEvents(MachineId, OccurredAtUtc DESC) INCLUDE (EventType, Severity);
CREATE INDEX IX_MachineEvents_Severity_AcknowledgedAtUtc ON iot.MachineEvents(Severity, AcknowledgedAtUtc) WHERE AcknowledgedAtUtc IS NULL;

-- MachineTelemetry
CREATE INDEX IX_MachineTelemetry_MachineId_MetricName_RecordedAtUtc ON iot.MachineTelemetry(MachineId, MetricName, RecordedAtUtc DESC);

-- MachineCycles
CREATE INDEX IX_MachineCycles_ProductionOrderId_CycleDateUtc ON iot.MachineCycles(ProductionOrderId, CycleDateUtc);
CREATE INDEX IX_MachineCycles_MachineId_CycleDateUtc ON iot.MachineCycles(MachineId, CycleDateUtc);

-- OeeCalculations
CREATE INDEX IX_OeeCalculations_MachineId_PeriodDate ON analytics.OeeCalculations(MachineId, PeriodDate DESC) INCLUDE (OeeValue, Availability, Performance, Quality);
CREATE INDEX IX_OeeCalculations_TenantId_PeriodDate_PeriodType ON analytics.OeeCalculations(TenantId, PeriodDate, PeriodType);

-- KpiValues
CREATE INDEX IX_KpiValues_KpiDefinitionId_PeriodDate ON analytics.KpiValues(KpiDefinitionId, PeriodDate DESC);
CREATE INDEX IX_KpiValues_MachineId_PeriodDate ON analytics.KpiValues(MachineId, PeriodDate DESC) WHERE MachineId IS NOT NULL;

-- AuditLogs
CREATE INDEX IX_AuditLogs_TenantId_OccurredAtUtc ON audit.AuditLogs(TenantId, OccurredAtUtc DESC);
CREATE INDEX IX_AuditLogs_EntityType_EntityId ON audit.AuditLogs(EntityType, EntityId);

-- AppUsers
CREATE INDEX IX_AppUsers_TenantId_IsActive ON security.AppUsers(TenantId, IsActive) INCLUDE (DisplayName, UserPrincipalName);

-- TeamsNotifications
CREATE INDEX IX_TeamsNotifications_Status_CreatedAtUtc ON integration.TeamsNotifications(Status, CreatedAtUtc) WHERE Status = 'Pending';
GO

-- =============================================================================
-- VIEWS FOR DASHBOARDS
-- =============================================================================

CREATE OR ALTER VIEW analytics.vw_MachineCurrentStatus
AS
SELECT
    m.MachineId,
    m.TenantId,
    m.MachineCode,
    m.MachineName,
    m.MachineType,
    d.DepartmentCode,
    d.DepartmentName,
    pl.LineName,
    m.CurrentStatus,
    m.LastStatusChangedAtUtc,
    ms.StateName          AS CurrentStateName,
    ms.StateCategory,
    ms.IsProductiveTime,
    ms.ColorHex,
    DATEDIFF(MINUTE, m.LastStatusChangedAtUtc, SYSUTCDATETIME()) AS MinutesInCurrentStatus,
    po.OrderNumber        AS ActiveProductionOrderNumber,
    p.ProductCode,
    p.ProductName
FROM mes.Machines m
INNER JOIN mes.Departments d ON m.DepartmentId = d.DepartmentId
LEFT  JOIN mes.ProductionLines pl ON m.ProductionLineId = pl.ProductionLineId
LEFT  JOIN iot.MachineStates ms ON m.CurrentStatus = ms.StateCode
LEFT  JOIN mes.ProductionOrders po ON po.MachineId = m.MachineId AND po.Status = 'InProgress'
LEFT  JOIN mes.Products p ON po.ProductId = p.ProductId
WHERE m.IsActive = 1;
GO

CREATE OR ALTER VIEW analytics.vw_WorkOrderProgress
AS
SELECT
    wo.WorkOrderId,
    wo.TenantId,
    wo.WorkOrderNumber,
    wo.Status,
    wo.Priority,
    wo.DueDate,
    wo.TotalQuantity,
    wo.CompletedQuantity,
    wo.RejectedQuantity,
    CASE WHEN wo.TotalQuantity > 0
         THEN CAST(wo.CompletedQuantity AS FLOAT) / CAST(wo.TotalQuantity AS FLOAT)
         ELSE 0 END AS CompletionRate,
    c.CustomerCode,
    c.CustomerName,
    COUNT(po.ProductionOrderId)  AS TotalProductionOrders,
    SUM(CASE WHEN po.Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedProductionOrders,
    wo.DueDate - CAST(SYSUTCDATETIME() AS DATE) AS DaysUntilDue
FROM mes.WorkOrders wo
INNER JOIN mes.Customers c ON wo.CustomerId = c.CustomerId
LEFT  JOIN mes.ProductionOrders po ON wo.WorkOrderId = po.WorkOrderId
WHERE wo.Status NOT IN ('Cancelled')
GROUP BY
    wo.WorkOrderId, wo.TenantId, wo.WorkOrderNumber, wo.Status,
    wo.Priority, wo.DueDate, wo.TotalQuantity, wo.CompletedQuantity,
    wo.RejectedQuantity, c.CustomerCode, c.CustomerName;
GO

CREATE OR ALTER VIEW analytics.vw_DailyOeeSummary
AS
SELECT
    o.TenantId,
    o.MachineId,
    m.MachineCode,
    m.MachineName,
    d.DepartmentCode,
    d.DepartmentName,
    o.PeriodDate,
    o.ShiftId,
    o.OeeValue,
    o.Availability,
    o.Performance,
    o.Quality,
    o.GoodQuantity,
    o.TotalQuantity,
    o.PlannedProductionTimeSeconds,
    o.UnscheduledDowntimeSeconds,
    o.CalculatedAtUtc
FROM analytics.OeeCalculations o
INNER JOIN mes.Machines m ON o.MachineId = m.MachineId
INNER JOIN mes.Departments d ON m.DepartmentId = d.DepartmentId
WHERE o.PeriodType = 'Shift';
GO

-- =============================================================================
-- STORED PROCEDURES — KPI / OEE CALCULATION
-- NOTE: This is a TEMPLATE. Validate business rules before production use.
-- =============================================================================

CREATE OR ALTER PROCEDURE analytics.usp_CalculateOeeForShift
    @MachineId          UNIQUEIDENTIFIER,
    @ShiftId            UNIQUEIDENTIFIER,
    @PeriodDate         DATE,
    @ProductionOrderId  UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @TenantId                    UNIQUEIDENTIFIER,
        @NominalCycleTimeSec         DECIMAL(18,4),
        @PlannedProductionTimeSec    BIGINT,
        @OperatingTimeSec            BIGINT,
        @RunTimeSec                  BIGINT,
        @TotalQty                    DECIMAL(18,4),
        @GoodQty                     DECIMAL(18,4),
        @ScheduledDowntimeSec        BIGINT,
        @UnscheduledDowntimeSec      BIGINT,
        @Availability                DECIMAL(10,6),
        @Performance                 DECIMAL(10,6),
        @Quality                     DECIMAL(10,6),
        @OeeValue                    DECIMAL(10,6);

    -- Retrieve machine context
    SELECT
        @TenantId            = TenantId,
        @NominalCycleTimeSec = NominalCycleTimeSeconds
    FROM mes.Machines WHERE MachineId = @MachineId;

    -- Retrieve shift planned time (minutes to seconds)
    SELECT @PlannedProductionTimeSec =
        (DATEDIFF(MINUTE,
            CAST(@PeriodDate AS DATETIME2) + CAST(s.StartTime AS DATETIME2),
            CAST(@PeriodDate AS DATETIME2) + CAST(s.EndTime AS DATETIME2)
        ) - s.BreakMinutes) * 60
    FROM mes.Shifts s WHERE s.ShiftId = @ShiftId;

    -- Aggregate downtime from status history
    SELECT
        @ScheduledDowntimeSec =
            SUM(CASE WHEN ms.IsPlannedStop = 1 THEN DATEDIFF(SECOND, h.StartedAtUtc, ISNULL(h.EndedAtUtc, SYSUTCDATETIME())) ELSE 0 END),
        @UnscheduledDowntimeSec =
            SUM(CASE WHEN ms.IsProductiveTime = 0 AND ms.IsPlannedStop = 0 THEN DATEDIFF(SECOND, h.StartedAtUtc, ISNULL(h.EndedAtUtc, SYSUTCDATETIME())) ELSE 0 END)
    FROM iot.MachineStatusHistory h
    INNER JOIN iot.MachineStates ms ON h.MachineStateId = ms.MachineStateId
    WHERE h.MachineId = @MachineId
      AND h.ShiftId   = @ShiftId
      AND h.EventDateUtc = @PeriodDate;

    SET @ScheduledDowntimeSec   = ISNULL(@ScheduledDowntimeSec, 0);
    SET @UnscheduledDowntimeSec = ISNULL(@UnscheduledDowntimeSec, 0);
    SET @OperatingTimeSec = @PlannedProductionTimeSec - @ScheduledDowntimeSec - @UnscheduledDowntimeSec;
    SET @RunTimeSec       = @OperatingTimeSec; -- Adjust with setup time if tracked separately

    -- Aggregate cycle quantities
    SELECT
        @TotalQty = SUM(c.GoodQuantity + c.ScrapQuantity),
        @GoodQty  = SUM(c.GoodQuantity)
    FROM iot.MachineCycles c
    WHERE c.MachineId = @MachineId
      AND c.CycleDateUtc = @PeriodDate
      AND (@ProductionOrderId IS NULL OR c.ProductionOrderId = @ProductionOrderId);

    SET @TotalQty = ISNULL(@TotalQty, 0);
    SET @GoodQty  = ISNULL(@GoodQty, 0);

    -- OEE components (ISO 22400)
    -- Availability = OperatingTime / PlannedProductionTime
    IF @PlannedProductionTimeSec > 0
        SET @Availability = CAST(@OperatingTimeSec AS DECIMAL(18,6)) / @PlannedProductionTimeSec;
    ELSE
        SET @Availability = 0;

    -- Performance = (TotalQty * IdealCycleTime) / OperatingTime
    IF @OperatingTimeSec > 0 AND @NominalCycleTimeSec > 0
        SET @Performance = (@TotalQty * @NominalCycleTimeSec) / @OperatingTimeSec;
    ELSE
        SET @Performance = 0;

    SET @Performance = CASE WHEN @Performance > 1 THEN 1 ELSE @Performance END;

    -- Quality = GoodQty / TotalQty
    IF @TotalQty > 0
        SET @Quality = @GoodQty / @TotalQty;
    ELSE
        SET @Quality = 0;

    SET @OeeValue = @Availability * @Performance * @Quality;

    -- Upsert OEE record
    MERGE analytics.OeeCalculations AS tgt
    USING (
        SELECT @MachineId AS MachineId, @ShiftId AS ShiftId,
               @PeriodDate AS PeriodDate, 'Shift' AS PeriodType,
               @ProductionOrderId AS ProductionOrderId
    ) AS src
    ON tgt.MachineId = src.MachineId
       AND tgt.ShiftId   = src.ShiftId
       AND tgt.PeriodDate = src.PeriodDate
       AND tgt.PeriodType = src.PeriodType
    WHEN MATCHED THEN
        UPDATE SET
            Availability                = @Availability,
            Performance                 = @Performance,
            Quality                     = @Quality,
            OeeValue                    = @OeeValue,
            PlannedProductionTimeSeconds = @PlannedProductionTimeSec,
            OperatingTimeSeconds        = @OperatingTimeSec,
            RunTimeSeconds              = @RunTimeSec,
            GoodQuantity                = @GoodQty,
            TotalQuantity               = @TotalQty,
            ScheduledDowntimeSeconds    = @ScheduledDowntimeSec,
            UnscheduledDowntimeSeconds  = @UnscheduledDowntimeSec,
            CalculatedAtUtc             = SYSUTCDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (TenantId, MachineId, ShiftId, ProductionOrderId, PeriodDate, PeriodType,
                PlannedProductionTimeSeconds, OperatingTimeSeconds, RunTimeSeconds,
                GoodQuantity, TotalQuantity,
                Availability, Performance, Quality, OeeValue,
                ScheduledDowntimeSeconds, UnscheduledDowntimeSeconds)
        VALUES (@TenantId, @MachineId, @ShiftId, @ProductionOrderId, @PeriodDate, 'Shift',
                @PlannedProductionTimeSec, @OperatingTimeSec, @RunTimeSec,
                @GoodQty, @TotalQty,
                @Availability, @Performance, @Quality, @OeeValue,
                @ScheduledDowntimeSec, @UnscheduledDowntimeSec);
END;
GO

-- =============================================================================
-- SEED DATA
-- =============================================================================

-- Machine States (ISO 22400 aligned)
INSERT INTO iot.MachineStates (StateCode, StateName, StateCategory, IsProductiveTime, IsPlannedStop, ColorHex, SortOrder)
VALUES
    ('Running',     'Running',              'Productive',  1, 0, '#22C55E', 1),
    ('Idle',        'Idle',                 'Productive',  1, 0, '#A3E635', 2),
    ('Setup',       'Setup / Changeover',   'Planned',     0, 1, '#FACC15', 3),
    ('Maintenance', 'Scheduled Maintenance','Planned',     0, 1, '#60A5FA', 4),
    ('Stopped',     'Unplanned Stop',       'Unplanned',   0, 0, '#F87171', 5),
    ('Fault',       'Fault / Alarm',        'Unplanned',   0, 0, '#DC2626', 6),
    ('Unknown',     'Unknown',              'External',    0, 0, '#9CA3AF', 7);
GO

-- KPI Definitions (ISO 22400)
INSERT INTO analytics.KpiDefinitions (KpiCode, KpiName, KpiGroup, IsoReference, Formula, Unit, TargetValue, WarningThreshold, CriticalThreshold, IsHigherBetter)
VALUES
    ('OEE',         'Overall Equipment Effectiveness', 'OEE',         'ISO 22400-2',  'Availability * Performance * Quality',                        '%', 0.85, 0.70, 0.60, 1),
    ('AVAILABILITY','Availability',                    'OEE',         'ISO 22400-2',  'OperatingTime / PlannedProductionTime',                        '%', 0.90, 0.80, 0.70, 1),
    ('PERFORMANCE', 'Performance Efficiency',          'OEE',         'ISO 22400-2',  '(TotalQty * IdealCycleTime) / OperatingTime',                  '%', 0.95, 0.85, 0.75, 1),
    ('QUALITY',     'Quality Rate',                    'OEE',         'ISO 22400-2',  'GoodQty / TotalQty',                                          '%', 0.98, 0.95, 0.90, 1),
    ('MTBF',        'Mean Time Between Failures',      'Reliability', 'ISO 22400-2',  'OperatingTime / NumberOfFailures',                            'hours', NULL, NULL, NULL, 1),
    ('MTTR',        'Mean Time To Repair',             'Reliability', 'ISO 22400-2',  'TotalRepairTime / NumberOfRepairs',                           'hours', NULL, NULL, NULL, 0),
    ('SCRAP_RATE',  'Scrap Rate',                      'Quality',     'ISO 22400-2',  'ScrapQty / TotalQty',                                         '%', 0.02, 0.05, 0.10, 0),
    ('THROUGHPUT',  'Production Throughput',           'Capacity',    'ISO 22400-2',  'GoodQty / PlannedProductionTimeHours',                        'pcs/h', NULL, NULL, NULL, 1),
    ('UTILIZATION', 'Machine Utilization',             'Capacity',    'ISO 22400-2',  'RunTime / ScheduledTime',                                     '%', 0.80, 0.65, 0.50, 1),
    ('NEE',         'Net Equipment Effectiveness',     'OEE',         'ISO 22400-2',  'Availability * Quality',                                      '%', 0.85, 0.70, 0.60, 1);
GO

-- Default Permissions
INSERT INTO security.Permissions (PermissionCode, Module, Action, Description) VALUES
    ('WORKORDER_VIEW',          'WorkOrders',  'View',    'View work orders'),
    ('WORKORDER_CREATE',        'WorkOrders',  'Create',  'Create work orders'),
    ('WORKORDER_EDIT',          'WorkOrders',  'Edit',    'Edit work orders'),
    ('WORKORDER_DELETE',        'WorkOrders',  'Delete',  'Delete work orders'),
    ('MACHINE_VIEW',            'Machines',    'View',    'View machines'),
    ('MACHINE_EDIT',            'Machines',    'Edit',    'Edit machine configuration'),
    ('MACHINE_ACK_ALARM',       'Machines',    'Ack',     'Acknowledge machine alarms'),
    ('ANALYTICS_VIEW',          'Analytics',   'View',    'View analytics and KPIs'),
    ('ANALYTICS_EXPORT',        'Analytics',   'Export',  'Export analytics data'),
    ('ADMIN_USERS',             'Admin',       'Users',   'Manage users and roles'),
    ('ADMIN_CONFIGURATION',     'Admin',       'Config',  'Manage system configuration'),
    ('IOT_DEVICE_MANAGE',       'IoT',         'Manage',  'Manage IoT device registration'),
    ('INTEGRATION_M365_MANAGE', 'Integration', 'Manage',  'Manage Microsoft 365 integration');
GO