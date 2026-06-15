
-- =============================================================================
-- VIEWS FOR DASHBOARDS
-- =============================================================================


-- =============================================================================
-- STORED PROCEDURES â€” KPI / OEE CALCULATION
-- NOTE: This is a TEMPLATE. Validate business rules before production use.
-- =============================================================================

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

--Syntax Error: Incorrect syntax near 'NOT'.
--CREATE INDEX IX_WorkOrders_DueDate ON mes.WorkOrders(DueDate) WHERE Status NOT IN ('Completed','Cancelled');
--
---- ProductionOrders



GO
