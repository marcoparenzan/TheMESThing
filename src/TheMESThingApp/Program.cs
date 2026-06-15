using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using TheMESThingApp;
using TheMESThingAppLib;
using TheItemsThingLib;
using TheMESItemsThingLib.Services;
using TheMESThingAPIClientLib.Proxies.M365;
using TheMESThingAPIClientLib.Proxies.Mes;
using TheMESThingAPIClientLib.Services;
using TheMESThingApp.Components;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile(@"D:\configuration\TheMESThing\TheMESThingApp\appsettings.json");

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization();

builder.Services.AddTransient<ApiAuthHandler>();
builder.Services.AddHttpClient<IPowerBIService, PowerBIService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7195";

builder.Services.AddHttpClient<MesThingApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
}).AddHttpMessageHandler<ApiAuthHandler>();

void AddProxy<TClient, TImpl>()
    where TClient : class
    where TImpl : class, TClient =>
    builder.Services
        .AddHttpClient<TClient, TImpl>(client => client.BaseAddress = new Uri(apiBaseUrl))
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        })
        .AddHttpMessageHandler<ApiAuthHandler>();

// M365 proxies
AddProxy<IEmailService, EmailServiceProxy>();
AddProxy<ICalendarService, CalendarServiceProxy>();
AddProxy<IContactsService, ContactsServiceProxy>();
AddProxy<IDriveService, DriveServiceProxy>();

// MES proxies
AddProxy<ICustomerService, CustomerServiceProxy>();
AddProxy<IDepartmentService, DepartmentServiceProxy>();
AddProxy<IProductionLineService, ProductionLineServiceProxy>();
AddProxy<IMachineService, MachineServiceProxy>();
AddProxy<ISkillService, SkillServiceProxy>();
AddProxy<IMachineSkillService, MachineSkillServiceProxy>();
AddProxy<IOperatorService, OperatorServiceProxy>();
AddProxy<IOperatorSkillService, OperatorSkillServiceProxy>();
AddProxy<IShiftService, ShiftServiceProxy>();
AddProxy<IProductService, ProductServiceProxy>();
AddProxy<IWorkOrderService, WorkOrderServiceProxy>();
AddProxy<IProductionOrderService, ProductionOrderServiceProxy>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapGet("/account/login", (string? returnUrl) =>
    Results.Challenge(
        new AuthenticationProperties { RedirectUri = returnUrl ?? "/" },
        [OpenIdConnectDefaults.AuthenticationScheme])
).AllowAnonymous();

app.MapGet("/account/logout", () =>
    Results.SignOut(
        new AuthenticationProperties { RedirectUri = "/" },
        [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme])
).AllowAnonymous();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(
        typeof(TheMESThingAppLib.Customers).Assembly,
        typeof(The365ThingAppLib.Users).Assembly
    );

app.Run();
