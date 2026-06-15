Act as a senior Solution Architect specialized in MES, Industry 4.0, Azure, .NET, Microsoft 365 integrations, and industrial analytics.

I need to design a database and application architecture for a MES platform developed with .NET 10, Blazor, Azure IoT, and Microsoft 365.

All output must be in English.
All entity names, table names, column names, indexes, constraints, DTOs, services, modules, and code artifacts must use English names.

The system must cover the following areas:

1. Work Orders and Customers
   - Import/synchronization from Microsoft Teams or Microsoft 365
   - Customer master data
   - Work orders, production orders, priorities, due dates, progress tracking

2. Machines, Departments, and Resources
   - Import/synchronization from Teams or Microsoft 365
   - Production departments, lines, cells, machines, operators, and resources
   - Relationships between machines, departments, skills, and production capacity

3. Machine Status from IoT
   - Data ingestion from Azure IoT Hub
   - Machine states: Running, Stopped, Idle, Maintenance, Fault, Setup
   - Machine events, alarms, cycle times, downtime, good quantity, scrap quantity
   - Historical state tracking and time-based traceability

4. Analytics and KPIs
   - KPI calculation based on ISO 22400
   - OEE calculation: Availability, Performance, Quality
   - Operational and management dashboards
   - Aggregations by machine, department, work order, shift, period, and customer

5. Microsoft 365 Integration
   - Login with Microsoft Entra ID / Azure AD
   - Microsoft Calendar integration for planning and shifts
   - Microsoft Teams notifications for alarms, work order progress, and anomalies
   - Power Apps / Power Automate integration for approval and operational workflows

Technology stack:
- .NET 10
- Blazor Server
- Tailwind CSS for responsive UI styling and reusable design system components
- Entity Framework Core
- Azure SQL Database
- Azure IoT Hub
- Azure Functions or Worker Services
- Microsoft Graph
- Microsoft Teams
- Microsoft Entra ID
- Azure Monitor / Application Insights

Additional context:
- A Microsoft Entra ID Service Principal is already configured and available
- Microsoft Graph API permissions are already granted
- The application can use application permissions and managed identities where appropriate
- Authentication and authorization should leverage the existing Entra ID tenant and Service Principal configuration

Request:

Design a complete MES data model and system architecture, including:

- Main business entities
- Suggested database tables
- Main columns for each table
- Primary keys and foreign keys
- Relationships between entities
- Historical/event tables for IoT data
- KPI, OEE, and analytics tables
- Microsoft 365 integration tables
- Audit log, security, roles, and permissions tables
- Suggested views or materialized/reporting tables for dashboards
- Indexing strategy
- Partitioning strategy
- Time-series data strategy
- Data retention strategy

Also provide:

1. Architectural overview
2. Conceptual ER model
3. Logical table schema
4. Main relationships
5. Main data flows
6. Analytics strategy for ISO 22400 and OEE
7. Microsoft 365 integration strategy
8. Recommended Azure services
9. Recommended bounded contexts / application modules
10. Recommended .NET 10 Blazor project structure
11. Entity Framework Core implementation guidelines: Include guidance for integrating Tailwind CSS with Blazor, including project setup, component styling conventions, responsive layouts, dashboard UI patterns, reusable CSS utilities, and enterprise design system recommendations.
12. Security, scalability, observability, and maintenance best practices
13. Future evolution roadmap

Additional requirement:

Provide a complete Azure SQL Database script that includes:

- CREATE SCHEMA statements
- CREATE TABLE statements
- Primary keys
- Foreign keys
- Unique constraints
- Check constraints
- Default constraints
- Recommended indexes
- Suggested partitioning strategy for IoT/time-series tables
- Suggested views for dashboards
- Suggested stored procedures or functions for KPI/OEE calculation
- Seed data for machine states, KPI definitions, roles, and permissions
- Naming conventions suitable for an enterprise MES platform

The Azure SQL script must use English names only and follow clear naming conventions, for example:

- Schemas: mes, iot, analytics, integration, security, audit
- Tables: WorkOrders, Customers, Machines, Departments, MachineEvents, MachineStatusHistory, KpiDefinitions, OeeCalculations
- Columns: WorkOrderId, CustomerId, MachineId, CreatedAtUtc, UpdatedAtUtc, ExternalMicrosoft365Id
- Constraints: PK_TableName, FK_TableName_ReferencedTableName, UQ_TableName_ColumnName, CK_TableName_Rule
- Indexes: IX_TableName_ColumnName

Output format:

1. Executive summary
2. Target architecture
3. Database schema design
4. Azure SQL DDL script
5. KPI and OEE calculation approach
6. Microsoft 365 integration design
7. Azure IoT ingestion design
8. .NET 10 Blazor implementation guidance
9. Security and compliance recommendations
10. Scalability and performance recommendations
11. Future improvements

Write the answer in English with a practical enterprise architecture style.
Use tables where useful.
Provide production-oriented SQL, but clearly indicate any parts that are templates or should be adjusted before deployment.