using Microsoft.EntityFrameworkCore;
using TheMESThingData.Entities.Analytics;
using TheMESThingData.Entities.Iot;
using TheMESThingData.Entities.Mes;

namespace TheMESThingData;

public class TheMESThingDbContext(DbContextOptions<TheMESThingDbContext> options) : DbContext(options)
{
    // MES
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<ProductionLine> ProductionLines => Set<ProductionLine>();
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<MachineSkill> MachineSkills => Set<MachineSkill>();
    public DbSet<Operator> Operators => Set<Operator>();
    public DbSet<OperatorSkill> OperatorSkills => Set<OperatorSkill>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();

    // IoT
    public DbSet<MachineState> MachineStates => Set<MachineState>();
    public DbSet<MachineStatusHistory> MachineStatusHistories => Set<MachineStatusHistory>();
    public DbSet<MachineTelemetry> MachineTelemetries => Set<MachineTelemetry>();
    public DbSet<MachineEvent> MachineEvents => Set<MachineEvent>();
    public DbSet<MachineCycle> MachineCycles => Set<MachineCycle>();

    // Analytics
    public DbSet<KpiDefinition> KpiDefinitions => Set<KpiDefinition>();
    public DbSet<KpiValue> KpiValues => Set<KpiValue>();
    public DbSet<OeeCalculation> OeeCalculations => Set<OeeCalculation>();
    public DbSet<ShiftReport> ShiftReports => Set<ShiftReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── mes ───────────────────────────────────────────────────────────────
        modelBuilder.Entity<Customer>(e =>
        {
            e.ToTable("Customers", "mes");
            e.HasKey(c => c.CustomerId);
            e.Property(c => c.CustomerId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(c => c.CustomerCode).HasMaxLength(50);
            e.Property(c => c.CustomerName).HasMaxLength(300);
            e.Property(c => c.ContactEmail).HasMaxLength(300);
            e.Property(c => c.ContactPhone).HasMaxLength(50);
            e.Property(c => c.Address).HasMaxLength(500);
            e.Property(c => c.Country).HasMaxLength(100);
            e.Property(c => c.ExternalMicrosoft365Id).HasMaxLength(200);
            e.Property(c => c.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(c => c.UpdatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(c => new { c.TenantId, c.CustomerCode }).IsUnique();
        });

        modelBuilder.Entity<Department>(e =>
        {
            e.ToTable("Departments", "mes");
            e.HasKey(d => d.DepartmentId);
            e.Property(d => d.DepartmentId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(d => d.DepartmentCode).HasMaxLength(50);
            e.Property(d => d.DepartmentName).HasMaxLength(200);
            e.Property(d => d.CostCenter).HasMaxLength(50);
            e.Property(d => d.ExternalMicrosoft365Id).HasMaxLength(200);
            e.Property(d => d.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(d => d.UpdatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(d => new { d.TenantId, d.DepartmentCode }).IsUnique();
            e.HasOne(d => d.ParentDepartment).WithMany(d => d.ChildDepartments)
                .HasForeignKey(d => d.ParentDepartmentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductionLine>(e =>
        {
            e.ToTable("ProductionLines", "mes");
            e.HasKey(l => l.ProductionLineId);
            e.Property(l => l.ProductionLineId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(l => l.LineCode).HasMaxLength(50);
            e.Property(l => l.LineName).HasMaxLength(200);
            e.Property(l => l.LineType).HasMaxLength(50).HasDefaultValue("Assembly");
            e.Property(l => l.NominalCapacity).HasColumnType("decimal(18,4)");
            e.Property(l => l.CapacityUnit).HasMaxLength(50);
            e.Property(l => l.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(l => l.UpdatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(l => new { l.TenantId, l.LineCode }).IsUnique();
            e.HasOne(l => l.Department).WithMany(d => d.ProductionLines).HasForeignKey(l => l.DepartmentId);
        });

        modelBuilder.Entity<Machine>(e =>
        {
            e.ToTable("Machines", "mes");
            e.HasKey(m => m.MachineId);
            e.Property(m => m.MachineId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(m => m.MachineCode).HasMaxLength(50);
            e.Property(m => m.MachineName).HasMaxLength(200);
            e.Property(m => m.MachineType).HasMaxLength(100);
            e.Property(m => m.Manufacturer).HasMaxLength(200);
            e.Property(m => m.Model).HasMaxLength(200);
            e.Property(m => m.SerialNumber).HasMaxLength(100);
            e.Property(m => m.NominalCycleTimeSeconds).HasColumnType("decimal(18,4)");
            e.Property(m => m.NominalCapacityPerHour).HasColumnType("decimal(18,4)");
            e.Property(m => m.IoTDeviceId).HasMaxLength(200);
            e.Property(m => m.IoTHubConnectionState).HasMaxLength(50).HasDefaultValue("Unknown");
            e.Property(m => m.CurrentStatus).HasMaxLength(50).HasDefaultValue("Unknown");
            e.Property(m => m.ExternalMicrosoft365Id).HasMaxLength(200);
            e.Property(m => m.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(m => m.UpdatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(m => new { m.TenantId, m.MachineCode }).IsUnique();
            e.HasOne(m => m.Department).WithMany(d => d.Machines).HasForeignKey(m => m.DepartmentId);
            e.HasOne(m => m.ProductionLine).WithMany(l => l.Machines).HasForeignKey(m => m.ProductionLineId);
        });

        modelBuilder.Entity<Skill>(e =>
        {
            e.ToTable("Skills", "mes");
            e.HasKey(s => s.SkillId);
            e.Property(s => s.SkillId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(s => s.SkillCode).HasMaxLength(50);
            e.Property(s => s.SkillName).HasMaxLength(200);
            e.Property(s => s.Category).HasMaxLength(100);
            e.Property(s => s.Description).HasMaxLength(500);
            e.Property(s => s.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(s => new { s.TenantId, s.SkillCode }).IsUnique();
        });

        modelBuilder.Entity<MachineSkill>(e =>
        {
            e.ToTable("MachineSkills", "mes");
            e.HasKey(ms => ms.MachineSkillId);
            e.Property(ms => ms.MachineSkillId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.HasIndex(ms => new { ms.MachineId, ms.SkillId }).IsUnique();
            e.HasOne(ms => ms.Machine).WithMany(m => m.MachineSkills).HasForeignKey(ms => ms.MachineId);
            e.HasOne(ms => ms.Skill).WithMany(s => s.MachineSkills).HasForeignKey(ms => ms.SkillId);
        });

        modelBuilder.Entity<Operator>(e =>
        {
            e.ToTable("Operators", "mes");
            e.HasKey(o => o.OperatorId);
            e.Property(o => o.OperatorId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(o => o.OperatorCode).HasMaxLength(50);
            e.Property(o => o.FirstName).HasMaxLength(150);
            e.Property(o => o.LastName).HasMaxLength(150);
            e.Property(o => o.BadgeNumber).HasMaxLength(50);
            e.Property(o => o.ExternalMicrosoft365Id).HasMaxLength(200);
            e.Property(o => o.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(o => o.UpdatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(o => new { o.TenantId, o.OperatorCode }).IsUnique();
            e.HasOne(o => o.Department).WithMany(d => d.Operators).HasForeignKey(o => o.DepartmentId);
        });

        modelBuilder.Entity<OperatorSkill>(e =>
        {
            e.ToTable("OperatorSkills", "mes");
            e.HasKey(os => os.OperatorSkillId);
            e.Property(os => os.OperatorSkillId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.HasIndex(os => new { os.OperatorId, os.SkillId }).IsUnique();
            e.HasOne(os => os.Operator).WithMany(o => o.OperatorSkills).HasForeignKey(os => os.OperatorId);
            e.HasOne(os => os.Skill).WithMany(s => s.OperatorSkills).HasForeignKey(os => os.SkillId);
        });

        modelBuilder.Entity<Shift>(e =>
        {
            e.ToTable("Shifts", "mes");
            e.HasKey(s => s.ShiftId);
            e.Property(s => s.ShiftId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(s => s.ShiftCode).HasMaxLength(50);
            e.Property(s => s.ShiftName).HasMaxLength(200);
            e.Property(s => s.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(s => new { s.TenantId, s.ShiftCode }).IsUnique();
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("Products", "mes");
            e.HasKey(p => p.ProductId);
            e.Property(p => p.ProductId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(p => p.ProductCode).HasMaxLength(50);
            e.Property(p => p.ProductName).HasMaxLength(300);
            e.Property(p => p.UnitOfMeasure).HasMaxLength(20).HasDefaultValue("PCS");
            e.Property(p => p.CycleTimeSeconds).HasColumnType("decimal(18,4)");
            e.Property(p => p.SetupTimeSeconds).HasColumnType("decimal(18,4)");
            e.Property(p => p.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(p => p.UpdatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(p => new { p.TenantId, p.ProductCode }).IsUnique();
        });

        modelBuilder.Entity<WorkOrder>(e =>
        {
            e.ToTable("WorkOrders", "mes");
            e.HasKey(w => w.WorkOrderId);
            e.Property(w => w.WorkOrderId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(w => w.WorkOrderNumber).HasMaxLength(50);
            e.Property(w => w.Description).HasMaxLength(500);
            e.Property(w => w.Status).HasMaxLength(50).HasDefaultValue("Draft");
            e.Property(w => w.TotalQuantity).HasColumnType("decimal(18,4)");
            e.Property(w => w.CompletedQuantity).HasColumnType("decimal(18,4)");
            e.Property(w => w.RejectedQuantity).HasColumnType("decimal(18,4)");
            e.Property(w => w.ExternalMicrosoft365Id).HasMaxLength(200);
            e.Property(w => w.TeamsChannelId).HasMaxLength(200);
            e.Property(w => w.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(w => w.UpdatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(w => new { w.TenantId, w.WorkOrderNumber }).IsUnique();
            e.HasOne(w => w.Customer).WithMany(c => c.WorkOrders).HasForeignKey(w => w.CustomerId);
        });

        modelBuilder.Entity<ProductionOrder>(e =>
        {
            e.ToTable("ProductionOrders", "mes");
            e.HasKey(po => po.ProductionOrderId);
            e.Property(po => po.ProductionOrderId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(po => po.OrderNumber).HasMaxLength(50);
            e.Property(po => po.Status).HasMaxLength(50).HasDefaultValue("Planned");
            e.Property(po => po.PlannedQuantity).HasColumnType("decimal(18,4)");
            e.Property(po => po.GoodQuantity).HasColumnType("decimal(18,4)");
            e.Property(po => po.ScrapQuantity).HasColumnType("decimal(18,4)");
            e.Property(po => po.ReworkQuantity).HasColumnType("decimal(18,4)");
            e.Property(po => po.PlannedCycleTimeSeconds).HasColumnType("decimal(18,4)");
            e.Property(po => po.ActualCycleTimeSeconds).HasColumnType("decimal(18,4)");
            e.Property(po => po.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(po => po.UpdatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(po => new { po.TenantId, po.OrderNumber }).IsUnique();
            e.HasOne(po => po.WorkOrder).WithMany(w => w.ProductionOrders).HasForeignKey(po => po.WorkOrderId);
            e.HasOne(po => po.Product).WithMany(p => p.ProductionOrders).HasForeignKey(po => po.ProductId);
            e.HasOne(po => po.Machine).WithMany(m => m.ProductionOrders).HasForeignKey(po => po.MachineId);
            e.HasOne(po => po.ProductionLine).WithMany(l => l.ProductionOrders).HasForeignKey(po => po.ProductionLineId);
        });

        // ── iot ───────────────────────────────────────────────────────────────
        modelBuilder.Entity<MachineState>(e =>
        {
            e.ToTable("MachineStates", "iot");
            e.HasKey(ms => ms.MachineStateId);
            e.Property(ms => ms.StateCode).HasMaxLength(50);
            e.Property(ms => ms.StateName).HasMaxLength(100);
            e.Property(ms => ms.StateCategory).HasMaxLength(50);
            e.Property(ms => ms.ColorHex).HasMaxLength(7);
            e.HasIndex(ms => ms.StateCode).IsUnique();
        });

        modelBuilder.Entity<MachineStatusHistory>(e =>
        {
            e.ToTable("MachineStatusHistory", "iot");
            e.HasKey(h => new { h.MachineStatusHistoryId, h.EventDateUtc });
            e.Property(h => h.MachineStatusHistoryId).UseIdentityColumn();
            e.Property(h => h.ReasonCode).HasMaxLength(100);
            e.Property(h => h.Notes).HasMaxLength(500);
            e.Property(h => h.IoTMessageId).HasMaxLength(200);
            e.Property(h => h.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(h => h.EventDateUtc).HasDefaultValueSql("CAST(SYSUTCDATETIME() AS DATE)");
            e.HasOne(h => h.MachineState).WithMany(s => s.MachineStatusHistories)
                .HasForeignKey(h => h.MachineStateId);
        });

        modelBuilder.Entity<MachineTelemetry>(e =>
        {
            e.ToTable("MachineTelemetry", "iot");
            e.HasKey(t => new { t.MachineTelemetryId, t.TelemetryDateUtc });
            e.Property(t => t.MachineTelemetryId).UseIdentityColumn();
            e.Property(t => t.MetricName).HasMaxLength(100);
            e.Property(t => t.MetricUnit).HasMaxLength(50);
            e.Property(t => t.IoTMessageId).HasMaxLength(200);
            e.Property(t => t.TelemetryDateUtc).HasDefaultValueSql("CAST(SYSUTCDATETIME() AS DATE)");
        });

        modelBuilder.Entity<MachineEvent>(e =>
        {
            e.ToTable("MachineEvents", "iot");
            e.HasKey(ev => new { ev.MachineEventId, ev.EventDateUtc });
            e.Property(ev => ev.MachineEventId).UseIdentityColumn();
            e.Property(ev => ev.EventType).HasMaxLength(100);
            e.Property(ev => ev.EventCode).HasMaxLength(100);
            e.Property(ev => ev.Severity).HasMaxLength(50).HasDefaultValue("Info");
            e.Property(ev => ev.EventMessage).HasMaxLength(1000);
            e.Property(ev => ev.IoTMessageId).HasMaxLength(200);
            e.Property(ev => ev.EventDateUtc).HasDefaultValueSql("CAST(SYSUTCDATETIME() AS DATE)");
        });

        modelBuilder.Entity<MachineCycle>(e =>
        {
            e.ToTable("MachineCycles", "iot");
            e.HasKey(c => new { c.MachineCycleId, c.CycleDateUtc });
            e.Property(c => c.MachineCycleId).UseIdentityColumn();
            e.Property(c => c.GoodQuantity).HasColumnType("decimal(18,4)");
            e.Property(c => c.ScrapQuantity).HasColumnType("decimal(18,4)");
            e.Property(c => c.ScrapReasonCode).HasMaxLength(100);
            e.Property(c => c.IoTMessageId).HasMaxLength(200);
            e.Property(c => c.CycleDateUtc).HasDefaultValueSql("CAST(SYSUTCDATETIME() AS DATE)");
            e.Property(c => c.CycleDurationSeconds).HasComputedColumnSql("DATEDIFF(SECOND, CycleStartAtUtc, CycleEndAtUtc)", stored: true);
        });

        // ── analytics ─────────────────────────────────────────────────────────
        modelBuilder.Entity<KpiDefinition>(e =>
        {
            e.ToTable("KpiDefinitions", "analytics");
            e.HasKey(k => k.KpiDefinitionId);
            e.Property(k => k.KpiDefinitionId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(k => k.KpiCode).HasMaxLength(50);
            e.Property(k => k.KpiName).HasMaxLength(200);
            e.Property(k => k.KpiGroup).HasMaxLength(100);
            e.Property(k => k.IsoReference).HasMaxLength(50);
            e.Property(k => k.Formula).HasMaxLength(1000);
            e.Property(k => k.Unit).HasMaxLength(50);
            e.Property(k => k.TargetValue).HasColumnType("decimal(18,4)");
            e.Property(k => k.WarningThreshold).HasColumnType("decimal(18,4)");
            e.Property(k => k.CriticalThreshold).HasColumnType("decimal(18,4)");
            e.Property(k => k.Description).HasMaxLength(1000);
            e.HasIndex(k => k.KpiCode).IsUnique();
        });

        modelBuilder.Entity<KpiValue>(e =>
        {
            e.ToTable("KpiValues", "analytics");
            e.HasKey(kv => kv.KpiValueId);
            e.Property(kv => kv.KpiValueId).UseIdentityColumn();
            e.Property(kv => kv.PeriodType).HasMaxLength(20).HasDefaultValue("Day");
            e.Property(kv => kv.KpiValueAmount).HasColumnName("KpiValue").HasColumnType("decimal(18,6)");
            e.Property(kv => kv.KpiTarget).HasColumnType("decimal(18,6)");
            e.Property(kv => kv.VarianceFromTarget).HasColumnType("decimal(18,6)");
            e.Property(kv => kv.CalculatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasOne(kv => kv.KpiDefinition).WithMany(k => k.KpiValues).HasForeignKey(kv => kv.KpiDefinitionId);
        });

        modelBuilder.Entity<OeeCalculation>(e =>
        {
            e.ToTable("OeeCalculations", "analytics");
            e.HasKey(o => o.OeeCalculationId);
            e.Property(o => o.OeeCalculationId).UseIdentityColumn();
            e.Property(o => o.PeriodType).HasMaxLength(20).HasDefaultValue("Shift");
            e.Property(o => o.PlannedQuantity).HasColumnType("decimal(18,4)");
            e.Property(o => o.GoodQuantity).HasColumnType("decimal(18,4)");
            e.Property(o => o.TotalQuantity).HasColumnType("decimal(18,4)");
            e.Property(o => o.Availability).HasColumnType("decimal(10,6)");
            e.Property(o => o.Performance).HasColumnType("decimal(10,6)");
            e.Property(o => o.Quality).HasColumnType("decimal(10,6)");
            e.Property(o => o.OeeValue).HasColumnType("decimal(10,6)");
            e.Property(o => o.CalculatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<ShiftReport>(e =>
        {
            e.ToTable("ShiftReports", "analytics");
            e.HasKey(sr => sr.ShiftReportId);
            e.Property(sr => sr.ShiftReportId).UseIdentityColumn();
            e.Property(sr => sr.GoodQuantity).HasColumnType("decimal(18,4)");
            e.Property(sr => sr.ScrapQuantity).HasColumnType("decimal(18,4)");
            e.Property(sr => sr.OeeValue).HasColumnType("decimal(10,6)");
            e.Property(sr => sr.Notes).HasMaxLength(1000);
            e.Property(sr => sr.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
        });
    }
}
