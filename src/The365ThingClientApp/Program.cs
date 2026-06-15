using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using The365ThingClientApp.Components;
using The365ThingLib;
using TheItemsThingLib;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile(@"D:\configuration\TheMESThing\The365ThingClientApp\appsettings.json");

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
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
    .AddAdditionalAssemblies(typeof(The365ThingAppLib.Users).Assembly);

app.Run();
