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
| `TheMESThingContracts` | Class library | Domain contracts written in [Ontly](https://github.com/marcoparenzan/ontly) YAML (`Contracts/*.yaml`); C# types (scalars with constraints, physical quantities, DTOs) are generated at build time by `Ontly.Generators`, plus JSON converters for the wire format |
| `TheMESThingLib` | Class library | MES domain services (Customer, Machine, WorkOrder, …) |
| `TheMESThingAppLib` | Razor class library | MES pages for the Blazor frontend, the IoT device simulator, the Live report and the *Chiedi a Ralf?* chat |
| `TheMESThingAPIClientLib` | Class library | Typed HTTP client for consuming the REST API (MES domain objects are the Ontly contract types) |
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

#### Typed telemetry and production counters

Ingestion endpoints whose bodies are Ontly contract types: a quantity is sent as a `[value, "unit"]` pair in its **canonical unit** (for example `[293.15, "K"]`), and invalid values are rejected before reaching the database. Readings are stored in the same `MachineTelemetry` table as the generic endpoint.

| Endpoint | Purpose |
| --- | --- |
| `POST /api/iot/telemetry/readings/{temperature\|line-speed\|vibration\|cycle-time}` | One reading of a metric (K, m/min, m/s², s) |
| `POST /api/iot/telemetry/readings/counters` | Production counters: planned production time, operating time, runtime, planned/total/good quantity and the reference period. The API computes the OEE with the Python plugin `oee.py` and stores counters and OEE in `OeeCalculations` |
| `POST /api/iot/machines/status` | Status reported by a device (Running, Idle, Down, Offline); updates the machine's current status |
| `GET /api/iot/machines/live-status` | Reported status, last telemetry time, online flag and effective status of every machine. A machine with no telemetry in the last 30 s (`onlineWithinSeconds`) is `Offline` |

### Analytics — `/api/analytics`

| Tag | Prefix | Notes |
| --- | --- | --- |
| Analytics – KpiDefinitions | `/api/analytics/kpi-definitions` | KPI catalogue with ISO 22400 reference, formula, and thresholds |
| Analytics – KpiValues | `/api/analytics/kpi-values` | Recorded KPI values by period, machine, and tenant |
| Analytics – OeeCalculations | `/api/analytics/oee` | OEE (Availability × Performance × Quality) with MTBF/MTTR |
| Analytics – ShiftReports | `/api/analytics/shift-reports` | Aggregated shift summaries with runtime, downtime, and OEE |

| Endpoint | Notes |
| --- | --- |
| `POST /api/analytics/oee/calculate` | Computes availability, performance, quality and OEE from the given times and quantities by running the Python plugin `oee.py` |

### Python plugins — `/api/plugins`

Business rules can be written as Python scripts under `TheMESThingAPI/plugins/` and run inside the API process by [PySharp](https://github.com/marcoparenzan/pysharp) (a Python interpreter for .NET; pure Python only, no CPython needed). `oee.py` holds the OEE formula.

| Endpoint | Notes |
| --- | --- |
| `POST /api/plugins/{name}/reload` | Re-reads a plugin from disk without restarting the API |

Plugins run in the API process without a sandbox: treat the `plugins/` folder as code.

### Assistant — `/api/assistant`

> Registered only when an AI provider is configured (see [Configuration](#configuration)).

| Endpoint | Notes |
| --- | --- |
| `POST /api/assistant/ask` | Natural-language questions (`{ "question": "...", "history": [...] }`). The model answers by calling tools that read machines with their live status, latest telemetry and latest OEE, and that calculate OEE with the Python plugin. The response lists the tools used |

### M365 — `/api/m365`

> M365 endpoints are only registered when the `M365` configuration section is present.

| Tag | Prefix | Operations |
| --- | --- | --- |
| M365 – Email | `/api/m365/email/{userId}/…` | List inbox/folders/messages, get, send, send-and-save, mark-read, delete |
| M365 – Calendar | `/api/m365/calendar/{userId}/events` | List (with time-window filter), get, create, update, delete |
| M365 – Users | `/api/m365/users` | List, search, get, create, update, disable, delete |
| M365 – Drive | `/api/m365/drive/{userId}/…` | List root/children, get item, download, create folder, upload, delete |

## Frontend

The Blazor app (sign-in with Microsoft Entra ID) offers, besides the MES CRUD pages:

- **Live report** (`/mes/live`) — a real-time report (refresh every second) built while Power BI is not available. It hosts an **IoT device simulator**: for each machine you start, the app posts temperature, line speed, vibration, cycle time, machine status and the production counters (planned/operating/run time, total/good quantity, reference period `Rolling300s`) to the API. The page reads back the stored readings and the OEE the API computed with the Python plugin.
- **Power BI** (`/mes/reports`) — embedded Power BI reports (`PowerBI:Reports` section of `appsettings.json`).
- **Chiedi a Ralf?** (`/mes/chat`) — a chat with the assistant of the API, with Markdown rendering. The simulator keeps running when the page is closed.

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
| IoT ingestion | Azure IoT Hub; typed readings via Ontly contracts; in-app device simulator for demos |
| Domain contracts | [Ontly](https://github.com/marcoparenzan/ontly) (YAML → C#, source generator) |
| Scripting | [PySharp](https://github.com/marcoparenzan/pysharp) (Python plugins in the API process) |
| AI assistant | Microsoft.Extensions.AI through `RalfAI.Providers` (OpenAI, Anthropic or Azure AI Foundry) |
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

### AI provider (assistant)

The assistant uses the same configuration as RalfAI. The API loads the RalfAI `config.json` from `RalfAI:ConfigPath`, else the `RALFAI_CONFIG_PATH` environment variable, else `D:\Configurations\RalfAI\config.json`; when that file does not exist it uses its own configuration instead. The endpoint is registered only when `AIProvider` is set:

```json
{
  "AIProvider": "foundry",
  "AzureAIFoundry": { "Endpoint": "<endpoint>", "ApiKey": "<key>", "ModelId": "<deployment>" }
}
```

`openai` (`OpenAI:ApiKey`, `OpenAI:ModelId`) and `anthropic` (`Anthropic:ApiKey`, `Anthropic:ModelId`) are also supported.

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

`TheMESThingContracts` restores `Ontly.Generators`, `TheMESThingAPI` restores `PySharp.Interpreter` and `RalfAI.Providers`; these packages come from a local NuGet feed (`D:\Dev\NuGetLocalFeed`) that must be registered as a NuGet source.

API interactive docs are available at `https://localhost:{port}/scalar/v1` in Development mode.

## Design notes

- Domain objects exchanged between API and clients are Ontly contract types: no primitive crosses the boundary, values carry their constraints (for example a maximum length) and quantities carry their unit. EF entities stay hand-written persistence types.
- All entity IDs use `NEWSEQUENTIALID()` (sequential GUIDs) to reduce index fragmentation on insert-heavy IoT tables.
- Multi-tenant support is built in: most entities carry a `TenantId` column, and unique indexes are scoped per tenant (e.g., `(TenantId, MachineCode)`).
- Entities that synchronise with Microsoft 365 carry an `ExternalMicrosoft365Id` column for bidirectional reference.
- Work orders also carry `TeamsChannelId` to enable Teams channel notifications.
- All timestamps are UTC (`DATETIME2`, `SYSUTCDATETIME()`).
