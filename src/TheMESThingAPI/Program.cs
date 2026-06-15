using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using The365ThingLib;
using TheItemsThingLib;
using TheMESThingAPI.Endpoints;
using TheMESThingData;
using TheMESItemsThingLib.Services;
using TheMESThingLib.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile(@"D:\configuration\TheMESThing\TheMESThingAPI\appsettings.json");

builder.Services.AddDbContext<TheMESThingDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("TheMESThing") ?? throw new InvalidOperationException("Connection string 'TheMESThingDb' not found.");
    options.UseSqlServer(connectionString);

    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
});

// Register MES services
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IProductionLineService, ProductionLineService>();
builder.Services.AddScoped<IMachineService, MachineService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IMachineSkillService, MachineSkillService>();
builder.Services.AddScoped<IOperatorService, OperatorService>();
builder.Services.AddScoped<IOperatorSkillService, OperatorSkillService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();
builder.Services.AddScoped<IProductionOrderService, ProductionOrderService>();

// Register M365 configuration
var m365Config = builder.Configuration.GetSection("M365").Get<M365Config>();
if (m365Config is not null)
{
    builder.Services.AddSingleton(m365Config);
    builder.Services.AddSingleton<IEmailService, M365EmailService>();
    builder.Services.AddSingleton<ICalendarService, M365CalendarService>();
    builder.Services.AddSingleton<IContactsService, M365ContactsService>();
    builder.Services.AddSingleton<IDriveService, M365DriveService>();
}

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.MaxDepth = 128;
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, ctx, ct) =>
    {
        doc.Info.Title = "TheMESThing API";
        doc.Info.Version = "v1";
        doc.Info.Description = "MES + IoT platform REST API";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "TheMESThing API";
        options.Theme = ScalarTheme.BluePlanet;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

app.Use(async (ctx, next) =>
{
    var expectedKey = ctx.RequestServices.GetRequiredService<IConfiguration>()["ApiKey"];
    if (!ctx.Request.Headers.TryGetValue("X-Api-Key", out var key) || key != expectedKey)
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return;
    }
    await next();
});

app.MapMesEndpoints();
app.MapIotEndpoints();
app.MapAnalyticsEndpoints();
if (m365Config is not null)
    app.MapM365Endpoints();

var dbContext = app.Services.CreateScope().ServiceProvider.GetRequiredService<TheMESThingDbContext>();
dbContext.Database.EnsureCreated();

app.Run();
