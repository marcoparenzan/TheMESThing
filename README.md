# TheMESThing

A Manufacturing Execution System (MES) platform built on .NET 10 that integrates Azure IoT Hub telemetry with Microsoft 365 services. The platform covers production management, real-time machine monitoring, OEE/KPI analytics, and M365 connectivity (Outlook, Calendar, OneDrive, Users).

## Architecture overview

```text
┌─────────────────────────────────────────────────┐
│              TheMESThingApp (Blazor)             │  ← MES frontend
└──────────────────────┬──────────────────────────┘
                       │ HTTP + API key
┌──────────────────────▼──────────────────────────┐
│            TheMESThingAPI (ASP.NET Core)         │  ← REST API
│  /api/mes  /api/iot  /api/analytics  /api/m365  │
└──────────────────────┬──────────────────────────┘
          ┌────────────┼────────────┐
          ▼            ▼            ▼
  TheMESThingData  TheMESThingLib  The365ThingLib
  (EF Core / SQL)  (services)     (Graph API)
          │
          ▼
   Azure SQL Database
   (schemas: mes, iot, analytics, integration, security, audit)
```

## Solutions

| Solution | Projects | Purpose |
| --- | --- | --- |
| `TheMESThing.slnx` | API, Data, Lib, App, ItemsLib | Core MES platform |
| `The365Thing.slnx` | 365Lib, 365AppLib, 365ClientApp, ItemsLib | M365 integration |

## Projects

| Project | Type | Description |
| --- | --- | --- |
| `TheMESThingAPI` | ASP.NET Core Web API | Minimal-API REST backend; exposes all four API groups |
| `TheMESThingApp` | Blazor Server | Interactive MES web frontend |
| `TheMESThingData` | Class library | `TheMESThingDbContext` (EF Core 10), all entity types |
| `TheMESThingLib` | Class library | MES domain services (Customer, Machine, WorkOrder, …) |
| `TheMESThingAppLib` | Class library | Application-layer services for the Blazor frontend |
| `TheMESThingAPIClientLib` | Class library | Typed HTTP client for consuming the REST API |
| `TheMESItemsThingLib` | Class library | Service abstractions shared between API and MES lib |
| `The365ThingLib` | Class library | Microsoft Graph wrappers (Email, Calendar, Contacts, Drive) |
| `The365ThingAppLib` | Class library | Application-layer services for M365 features |
| `The365ThingClientApp` | Blazor | M365 client application |
| `TheItemsThingLib` | Class library | Shared DTOs (User, Email, CalendarEvent, DriveItem, …) |

## REST API

Authentication is enforced by an `X-Api-Key` header validated in a middleware pipeline. All endpoints return JSON; collection endpoints return a paginated `PagedResult<T>`.

### MES — `/api/mes`

| Tag | Prefix | Resources |
| --- | --- | --- |
| MES – Customers | `/api/mes/customers` | CRUD for customer master data |
| MES – Departments | `/api/mes/departments` | CRUD with hierarchical parent/child support |
| MES – ProductionLines | `/api/mes/production-lines` | CRUD, linked to a Department |
| MES – Machines | `/api/mes/machines` | CRUD with IoT device binding, linked to Department and ProductionLine |
| MES – Skills | `/api/mes/skills` | Skill catalogue |
| MES – MachineSkills | `/api/mes/machine-skills` | Assign/remove skills for machines |
| MES – Operators | `/api/mes/operators` | CRUD, optionally linked to Microsoft 365 user |
| MES – OperatorSkills | `/api/mes/operator-skills` | Assign skills with proficiency levels |
| MES – Shifts | `/api/mes/shifts` | Shift definitions |
| MES – Products | `/api/mes/products` | Product catalogue with cycle time and setup time |
| MES – WorkOrders | `/api/mes/work-orders` | Work orders (with status validation) linked to Customers |
| MES – ProductionOrders | `/api/mes/production-orders` | Production orders linked to WorkOrder, Product, Machine |

**WorkOrder statuses:** `Draft`, `Released`, `InProgress`, `OnHold`, `Completed`, `Cancelled`

### IoT — `/api/iot`

| Tag | Prefix | Notes |
| --- | --- | --- |
| IoT – MachineStates | `/api/iot/machine-states` | Reference table of machine states (Running, Stopped, Idle, …) |
| IoT – MachineStatusHistory | `/api/iot/machine-status-history` | Time-series state transitions; filterable by machine and date range |
| IoT – MachineTelemetry | `/api/iot/telemetry` | Raw sensor metrics; filterable by machine, metric name, and date range |
| IoT – MachineEvents | `/api/iot/machine-events` | Alarms and events; supports acknowledgement via `PATCH /{id}/acknowledge` |
| IoT – MachineCycles | `/api/iot/machine-cycles` | Per-cycle good/scrap quantities with computed duration |

### Analytics — `/api/analytics`

| Tag | Prefix | Notes |
| --- | --- | --- |
| Analytics – KpiDefinitions | `/api/analytics/kpi-definitions` | KPI catalogue with ISO 22400 reference, formula, and thresholds |
| Analytics – KpiValues | `/api/analytics/kpi-values` | Recorded KPI values by period, machine, and tenant |
| Analytics – OeeCalculations | `/api/analytics/oee` | OEE (Availability × Performance × Quality) with MTBF/MTTR |
| Analytics – ShiftReports | `/api/analytics/shift-reports` | Aggregated shift summaries with runtime, downtime, and OEE |

### M365 — `/api/m365`

> M365 endpoints are only registered when the `M365` configuration section is present.

| Tag | Prefix | Operations |
| --- | --- | --- |
| M365 – Email | `/api/m365/email/{userId}/…` | List inbox/folders/messages, get, send, send-and-save, mark-read, delete |
| M365 – Calendar | `/api/m365/calendar/{userId}/events` | List (with time-window filter), get, create, update, delete |
| M365 – Users | `/api/m365/users` | List, search, get, create, update, disable, delete |
| M365 – Drive | `/api/m365/drive/{userId}/…` | List root/children, get item, download, create folder, upload, delete |

## Data model

Entity Framework Core 10 maps entities to Azure SQL schemas:

| Schema | Tables |
| --- | --- |
| `mes` | Customers, Departments, ProductionLines, Machines, Skills, MachineSkills, Operators, OperatorSkills, Shifts, Products, WorkOrders, ProductionOrders |
| `iot` | MachineStates, MachineStatusHistory, MachineTelemetry, MachineEvents, MachineCycles |
| `analytics` | KpiDefinitions, KpiValues, OeeCalculations, ShiftReports |
| `integration` | (reserved for M365 sync state) |
| `security` | Tenants, Roles, … |
| `audit` | Audit log tables |

IoT/time-series tables use composite primary keys `(id, date)` to support partitioning. `MachineCycle.CycleDurationSeconds` is a persisted computed column (`DATEDIFF(SECOND, CycleStartAtUtc, CycleEndAtUtc)`). The full DDL for all schemas is in [db.sql](db.sql).

## Technology stack

| Layer | Technology |
| --- | --- |
| Runtime | .NET 10 |
| API framework | ASP.NET Core 10 Minimal APIs |
| Frontend | Blazor Server |
| ORM | Entity Framework Core 10 |
| Database | Azure SQL (with `NEWSEQUENTIALID()` PKs) |
| IoT ingestion | Azure IoT Hub |
| M365 integration | Microsoft Graph via `The365ThingLib` |
| Auth (API) | API key (`X-Api-Key` header) |
| Auth (M365) | Client credentials (Entra ID Service Principal) |
| API docs | Scalar (`/scalar/v1` in Development) |
| Observability | Azure Monitor / Application Insights |

## Configuration

### `appsettings.json` — TheMESThingAPI

```json
{
  "ConnectionStrings": {
    "TheMESThing": "<Azure SQL connection string>"
  },
  "ApiKey": "<secret api key>",
  "M365": {
    "TenantId": "<Entra ID tenant GUID>",
    "ClientId": "<Service Principal app GUID>",
    "ClientSecret": "<client secret>"
  }
}
```

The `M365` section is optional. When absent, the M365 services and endpoints are not registered.

## Getting started

**Prerequisites:** .NET 10 SDK, Azure SQL Database (or SQL Server 2022+).

```bash
# Restore and build
dotnet build src/TheMESThing.slnx

# Run the API (creates the DB schema on first start via EnsureCreated)
dotnet run --project src/TheMESThingAPI

# Run the Blazor frontend
dotnet run --project src/TheMESThingApp
```

API interactive docs are available at `https://localhost:{port}/scalar/v1` in Development mode.

## Design notes

- All entity IDs use `NEWSEQUENTIALID()` (sequential GUIDs) to reduce index fragmentation on insert-heavy IoT tables.
- Multi-tenant support is built in: most entities carry a `TenantId` column, and unique indexes are scoped per tenant (e.g., `(TenantId, MachineCode)`).
- Entities that synchronise with Microsoft 365 carry an `ExternalMicrosoft365Id` column for bidirectional reference.
- Work orders also carry `TeamsChannelId` to enable Teams channel notifications.
- All timestamps are UTC (`DATETIME2`, `SYSUTCDATETIME()`).
