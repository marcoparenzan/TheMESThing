using Microsoft.AspNetCore.Http.HttpResults;
using TheMESItemsThingLib.Services;
using TheMESThingAPI.Common;
using TheMESThingData.Entities.Mes;

namespace TheMESThingAPI.Endpoints;

public static class MesEndpoints
{
    public static IEndpointRouteBuilder MapMesEndpoints(this IEndpointRouteBuilder app)
    {
        MapCustomers(app);
        MapDepartments(app);
        MapProductionLines(app);
        MapMachines(app);
        MapSkills(app);
        MapMachineSkills(app);
        MapOperators(app);
        MapOperatorSkills(app);
        MapShifts(app);
        MapProducts(app);
        MapWorkOrders(app);
        MapProductionOrders(app);
        return app;
    }

    // ── Customers ─────────────────────────────────────────────────────────────
    static void MapCustomers(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/mes/customers").WithTags("MES – Customers");

        g.MapGet("/", async (ICustomerService svc, Guid? tenantId, int page = 1, int pageSize = 20) =>
            await svc.GetListAsync(tenantId, page, pageSize))
        .WithName("GetCustomers").WithSummary("List customers");

        g.MapGet("/{id:guid}", async Task<Results<Ok<Customer>, NotFound>> (Guid id, ICustomerService svc) =>
        {
            var e = await svc.GetByIdAsync(id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetCustomer").WithSummary("Get a customer by ID");

        g.MapPost("/", async Task<Created<Customer>> (Customer body, ICustomerService svc) =>
        {
            var created = await svc.CreateAsync(body);
            return TypedResults.Created($"/api/mes/customers/{created.CustomerId}", created);
        })
        .WithName("CreateCustomer").WithSummary("Create a customer");

        g.MapPut("/{id:guid}", async Task<Results<Ok<Customer>, NotFound>> (Guid id, Customer body, ICustomerService svc) =>
        {
            var updated = await svc.UpdateAsync(id, body);
            return updated is null ? TypedResults.NotFound() : TypedResults.Ok(updated);
        })
        .WithName("UpdateCustomer").WithSummary("Update a customer");

        g.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, ICustomerService svc) =>
        {
            var deleted = await svc.DeleteAsync(id);
            return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName("DeleteCustomer").WithSummary("Delete a customer");
    }

    // ── Departments ───────────────────────────────────────────────────────────
    static void MapDepartments(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/mes/departments").WithTags("MES – Departments");

        g.MapGet("/", async (IDepartmentService svc, Guid? tenantId, int page = 1, int pageSize = 20) =>
            await svc.GetListAsync(tenantId, page, pageSize))
        .WithName("GetDepartments").WithSummary("List departments");

        g.MapGet("/{id:guid}", async Task<Results<Ok<Department>, NotFound>> (Guid id, IDepartmentService svc) =>
        {
            var e = await svc.GetByIdAsync(id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetDepartment").WithSummary("Get a department by ID");

        g.MapPost("/", async Task<Created<Department>> (Department body, IDepartmentService svc) =>
        {
            var created = await svc.CreateAsync(body);
            return TypedResults.Created($"/api/mes/departments/{created.DepartmentId}", created);
        })
        .WithName("CreateDepartment").WithSummary("Create a department");

        g.MapPut("/{id:guid}", async Task<Results<Ok<Department>, NotFound>> (Guid id, Department body, IDepartmentService svc) =>
        {
            var updated = await svc.UpdateAsync(id, body);
            return updated is null ? TypedResults.NotFound() : TypedResults.Ok(updated);
        })
        .WithName("UpdateDepartment").WithSummary("Update a department");

        g.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, IDepartmentService svc) =>
        {
            var deleted = await svc.DeleteAsync(id);
            return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName("DeleteDepartment").WithSummary("Delete a department");
    }

    // ── ProductionLines ───────────────────────────────────────────────────────
    static void MapProductionLines(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/mes/production-lines").WithTags("MES – ProductionLines");

        g.MapGet("/", async (IProductionLineService svc, Guid? tenantId, int page = 1, int pageSize = 20) =>
            await svc.GetListAsync(tenantId, page, pageSize))
        .WithName("GetProductionLines").WithSummary("List production lines");

        g.MapGet("/{id:guid}", async Task<Results<Ok<ProductionLine>, NotFound>> (Guid id, IProductionLineService svc) =>
        {
            var e = await svc.GetByIdAsync(id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetProductionLine").WithSummary("Get a production line by ID");

        g.MapPost("/", async Task<Created<ProductionLine>> (ProductionLine body, IProductionLineService svc) =>
        {
            var created = await svc.CreateAsync(body);
            return TypedResults.Created($"/api/mes/production-lines/{created.ProductionLineId}", created);
        })
        .WithName("CreateProductionLine").WithSummary("Create a production line");

        g.MapPut("/{id:guid}", async Task<Results<Ok<ProductionLine>, NotFound>> (Guid id, ProductionLine body, IProductionLineService svc) =>
        {
            var updated = await svc.UpdateAsync(id, body);
            return updated is null ? TypedResults.NotFound() : TypedResults.Ok(updated);
        })
        .WithName("UpdateProductionLine").WithSummary("Update a production line");

        g.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, IProductionLineService svc) =>
        {
            var deleted = await svc.DeleteAsync(id);
            return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName("DeleteProductionLine").WithSummary("Delete a production line");
    }

    // ── Machines ──────────────────────────────────────────────────────────────
    static void MapMachines(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/mes/machines").WithTags("MES – Machines");

        g.MapGet("/", async (IMachineService svc, Guid? tenantId, Guid? departmentId, int page = 1, int pageSize = 20) =>
            await svc.GetListAsync(tenantId, departmentId, page, pageSize))
        .WithName("GetMachines").WithSummary("List machines");

        g.MapGet("/{id:guid}", async Task<Results<Ok<Machine>, NotFound>> (Guid id, IMachineService svc) =>
        {
            var e = await svc.GetByIdAsync(id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetMachine").WithSummary("Get a machine by ID");

        g.MapPost("/", async Task<Created<Machine>> (Machine body, IMachineService svc) =>
        {
            var created = await svc.CreateAsync(body);
            return TypedResults.Created($"/api/mes/machines/{created.MachineId}", created);
        })
        .WithName("CreateMachine").WithSummary("Create a machine");

        g.MapPut("/{id:guid}", async Task<Results<Ok<Machine>, NotFound>> (Guid id, Machine body, IMachineService svc) =>
        {
            var updated = await svc.UpdateAsync(id, body);
            return updated is null ? TypedResults.NotFound() : TypedResults.Ok(updated);
        })
        .WithName("UpdateMachine").WithSummary("Update a machine");

        g.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, IMachineService svc) =>
        {
            var deleted = await svc.DeleteAsync(id);
            return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName("DeleteMachine").WithSummary("Delete a machine");
    }

    // ── Skills ────────────────────────────────────────────────────────────────
    static void MapSkills(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/mes/skills").WithTags("MES – Skills");

        g.MapGet("/", async (ISkillService svc, Guid? tenantId, int page = 1, int pageSize = 20) =>
            await svc.GetListAsync(tenantId, page, pageSize))
        .WithName("GetSkills").WithSummary("List skills");

        g.MapGet("/{id:guid}", async Task<Results<Ok<Skill>, NotFound>> (Guid id, ISkillService svc) =>
        {
            var e = await svc.GetByIdAsync(id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetSkill").WithSummary("Get a skill by ID");

        g.MapPost("/", async Task<Created<Skill>> (Skill body, ISkillService svc) =>
        {
            var created = await svc.CreateAsync(body);
            return TypedResults.Created($"/api/mes/skills/{created.SkillId}", created);
        })
        .WithName("CreateSkill").WithSummary("Create a skill");

        g.MapPut("/{id:guid}", async Task<Results<Ok<Skill>, NotFound>> (Guid id, Skill body, ISkillService svc) =>
        {
            var updated = await svc.UpdateAsync(id, body);
            return updated is null ? TypedResults.NotFound() : TypedResults.Ok(updated);
        })
        .WithName("UpdateSkill").WithSummary("Update a skill");

        g.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, ISkillService svc) =>
        {
            var deleted = await svc.DeleteAsync(id);
            return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName("DeleteSkill").WithSummary("Delete a skill");
    }

    // ── MachineSkills ─────────────────────────────────────────────────────────
    static void MapMachineSkills(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/mes/machine-skills").WithTags("MES – MachineSkills");

        g.MapGet("/", async (IMachineSkillService svc, Guid? machineId, int page = 1, int pageSize = 50) =>
            await svc.GetListAsync(machineId, page, pageSize))
        .WithName("GetMachineSkills").WithSummary("List machine-skill assignments");

        g.MapGet("/{id:guid}", async Task<Results<Ok<MachineSkill>, NotFound>> (Guid id, IMachineSkillService svc) =>
        {
            var e = await svc.GetByIdAsync(id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetMachineSkill").WithSummary("Get a machine-skill by ID");

        g.MapPost("/", async Task<Created<MachineSkill>> (MachineSkill body, IMachineSkillService svc) =>
        {
            var created = await svc.CreateAsync(body);
            return TypedResults.Created($"/api/mes/machine-skills/{created.MachineSkillId}", created);
        })
        .WithName("CreateMachineSkill").WithSummary("Assign a skill to a machine");

        g.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, IMachineSkillService svc) =>
        {
            var deleted = await svc.DeleteAsync(id);
            return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName("DeleteMachineSkill").WithSummary("Remove a skill from a machine");
    }

    // ── Operators ─────────────────────────────────────────────────────────────
    static void MapOperators(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/mes/operators").WithTags("MES – Operators");

        g.MapGet("/", async (IOperatorService svc, Guid? tenantId, Guid? departmentId, int page = 1, int pageSize = 20) =>
            await svc.GetListAsync(tenantId, departmentId, page, pageSize))
        .WithName("GetOperators").WithSummary("List operators");

        g.MapGet("/{id:guid}", async Task<Results<Ok<Operator>, NotFound>> (Guid id, IOperatorService svc) =>
        {
            var e = await svc.GetByIdAsync(id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetOperator").WithSummary("Get an operator by ID");

        g.MapPost("/", async Task<Created<Operator>> (Operator body, IOperatorService svc) =>
        {
            var created = await svc.CreateAsync(body);
            return TypedResults.Created($"/api/mes/operators/{created.OperatorId}", created);
        })
        .WithName("CreateOperator").WithSummary("Create an operator");

        g.MapPut("/{id:guid}", async Task<Results<Ok<Operator>, NotFound>> (Guid id, Operator body, IOperatorService svc) =>
        {
            var updated = await svc.UpdateAsync(id, body);
            return updated is null ? TypedResults.NotFound() : TypedResults.Ok(updated);
        })
        .WithName("UpdateOperator").WithSummary("Update an operator");

        g.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, IOperatorService svc) =>
        {
            var deleted = await svc.DeleteAsync(id);
            return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName("DeleteOperator").WithSummary("Delete an operator");
    }

    // ── OperatorSkills ────────────────────────────────────────────────────────
    static void MapOperatorSkills(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/mes/operator-skills").WithTags("MES – OperatorSkills");

        g.MapGet("/", async (IOperatorSkillService svc, Guid? operatorId, int page = 1, int pageSize = 50) =>
            await svc.GetListAsync(operatorId, page, pageSize))
        .WithName("GetOperatorSkills").WithSummary("List operator-skill assignments");

        g.MapGet("/{id:guid}", async Task<Results<Ok<OperatorSkill>, NotFound>> (Guid id, IOperatorSkillService svc) =>
        {
            var e = await svc.GetByIdAsync(id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetOperatorSkill").WithSummary("Get an operator-skill by ID");

        g.MapPost("/", async Task<Created<OperatorSkill>> (OperatorSkill body, IOperatorSkillService svc) =>
        {
            var created = await svc.CreateAsync(body);
            return TypedResults.Created($"/api/mes/operator-skills/{created.OperatorSkillId}", created);
        })
        .WithName("CreateOperatorSkill").WithSummary("Assign a skill to an operator");

        g.MapPut("/{id:guid}", async Task<Results<Ok<OperatorSkill>, NotFound>> (Guid id, OperatorSkill body, IOperatorSkillService svc) =>
        {
            var updated = await svc.UpdateAsync(id, body);
            return updated is null ? TypedResults.NotFound() : TypedResults.Ok(updated);
        })
        .WithName("UpdateOperatorSkill").WithSummary("Update operator skill proficiency");

        g.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, IOperatorSkillService svc) =>
        {
            var deleted = await svc.DeleteAsync(id);
            return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName("DeleteOperatorSkill").WithSummary("Remove a skill from an operator");
    }

    // ── Shifts ────────────────────────────────────────────────────────────────
    static void MapShifts(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/mes/shifts").WithTags("MES – Shifts");

        g.MapGet("/", async (IShiftService svc, Guid? tenantId, int page = 1, int pageSize = 20) =>
            await svc.GetListAsync(tenantId, page, pageSize))
        .WithName("GetShifts").WithSummary("List shifts");

        g.MapGet("/{id:guid}", async Task<Results<Ok<Shift>, NotFound>> (Guid id, IShiftService svc) =>
        {
            var e = await svc.GetByIdAsync(id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetShift").WithSummary("Get a shift by ID");

        g.MapPost("/", async Task<Created<Shift>> (Shift body, IShiftService svc) =>
        {
            var created = await svc.CreateAsync(body);
            return TypedResults.Created($"/api/mes/shifts/{created.ShiftId}", created);
        })
        .WithName("CreateShift").WithSummary("Create a shift");

        g.MapPut("/{id:guid}", async Task<Results<Ok<Shift>, NotFound>> (Guid id, Shift body, IShiftService svc) =>
        {
            var updated = await svc.UpdateAsync(id, body);
            return updated is null ? TypedResults.NotFound() : TypedResults.Ok(updated);
        })
        .WithName("UpdateShift").WithSummary("Update a shift");

        g.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, IShiftService svc) =>
        {
            var deleted = await svc.DeleteAsync(id);
            return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName("DeleteShift").WithSummary("Delete a shift");
    }

    // ── Products ──────────────────────────────────────────────────────────────
    static void MapProducts(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/mes/products").WithTags("MES – Products");

        g.MapGet("/", async (IProductService svc, Guid? tenantId, int page = 1, int pageSize = 20) =>
            await svc.GetListAsync(tenantId, page, pageSize))
        .WithName("GetProducts").WithSummary("List products");

        g.MapGet("/{id:guid}", async Task<Results<Ok<Product>, NotFound>> (Guid id, IProductService svc) =>
        {
            var e = await svc.GetByIdAsync(id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetProduct").WithSummary("Get a product by ID");

        g.MapPost("/", async Task<Created<Product>> (Product body, IProductService svc) =>
        {
            var created = await svc.CreateAsync(body);
            return TypedResults.Created($"/api/mes/products/{created.ProductId}", created);
        })
        .WithName("CreateProduct").WithSummary("Create a product");

        g.MapPut("/{id:guid}", async Task<Results<Ok<Product>, NotFound>> (Guid id, Product body, IProductService svc) =>
        {
            var updated = await svc.UpdateAsync(id, body);
            return updated is null ? TypedResults.NotFound() : TypedResults.Ok(updated);
        })
        .WithName("UpdateProduct").WithSummary("Update a product");

        g.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, IProductService svc) =>
        {
            var deleted = await svc.DeleteAsync(id);
            return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName("DeleteProduct").WithSummary("Delete a product");
    }

    // ── WorkOrders ────────────────────────────────────────────────────────────
    static void MapWorkOrders(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/mes/work-orders").WithTags("MES – WorkOrders");

        g.MapGet("/", async (IWorkOrderService svc, Guid? tenantId, string? status, int page = 1, int pageSize = 20) =>
            await svc.GetListAsync(tenantId, status, page, pageSize))
        .WithName("GetWorkOrders").WithSummary("List work orders");

        g.MapGet("/{id:guid}", async Task<Results<Ok<WorkOrder>, NotFound>> (Guid id, IWorkOrderService svc) =>
        {
            var e = await svc.GetByIdAsync(id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetWorkOrder").WithSummary("Get a work order by ID");

        g.MapPost("/", async Task<Results<Created<WorkOrder>, ValidationProblem>> (WorkOrder body, IWorkOrderService svc) =>
        {
            if (!WorkOrderStatuses.IsValid(body.Status))
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                    { [nameof(body.Status)] = [$"Status must be one of: {WorkOrderStatuses.AllowedValues}"] });
            var created = await svc.CreateAsync(body);
            return TypedResults.Created($"/api/mes/work-orders/{created.WorkOrderId}", created);
        })
        .WithName("CreateWorkOrder").WithSummary("Create a work order");

        g.MapPut("/{id:guid}", async Task<Results<Ok<WorkOrder>, NotFound, ValidationProblem>> (Guid id, WorkOrder body, IWorkOrderService svc) =>
        {
            if (!WorkOrderStatuses.IsValid(body.Status))
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                    { [nameof(body.Status)] = [$"Status must be one of: {WorkOrderStatuses.AllowedValues}"] });
            var updated = await svc.UpdateAsync(id, body);
            return updated is null ? TypedResults.NotFound() : TypedResults.Ok(updated);
        })
        .WithName("UpdateWorkOrder").WithSummary("Update a work order");

        g.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, IWorkOrderService svc) =>
        {
            var deleted = await svc.DeleteAsync(id);
            return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName("DeleteWorkOrder").WithSummary("Delete a work order");
    }

    // ── ProductionOrders ──────────────────────────────────────────────────────
    static void MapProductionOrders(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/mes/production-orders").WithTags("MES – ProductionOrders");

        g.MapGet("/", async (IProductionOrderService svc, Guid? tenantId, Guid? machineId, string? status, int page = 1, int pageSize = 20) =>
            await svc.GetListAsync(tenantId, machineId, status, page, pageSize))
        .WithName("GetProductionOrders").WithSummary("List production orders");

        g.MapGet("/{id:guid}", async Task<Results<Ok<ProductionOrder>, NotFound>> (Guid id, IProductionOrderService svc) =>
        {
            var e = await svc.GetByIdAsync(id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetProductionOrder").WithSummary("Get a production order by ID");

        g.MapPost("/", async Task<Created<ProductionOrder>> (ProductionOrder body, IProductionOrderService svc) =>
        {
            var created = await svc.CreateAsync(body);
            return TypedResults.Created($"/api/mes/production-orders/{created.ProductionOrderId}", created);
        })
        .WithName("CreateProductionOrder").WithSummary("Create a production order");

        g.MapPut("/{id:guid}", async Task<Results<Ok<ProductionOrder>, NotFound>> (Guid id, ProductionOrder body, IProductionOrderService svc) =>
        {
            var updated = await svc.UpdateAsync(id, body);
            return updated is null ? TypedResults.NotFound() : TypedResults.Ok(updated);
        })
        .WithName("UpdateProductionOrder").WithSummary("Update a production order");

        g.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, IProductionOrderService svc) =>
        {
            var deleted = await svc.DeleteAsync(id);
            return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName("DeleteProductionOrder").WithSummary("Delete a production order");
    }
}
