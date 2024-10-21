using HNOne.Web.Commons;
using HNOne.Web.Components;
using HNOne.Web.Extensions;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);
string apiUrl = builder.Configuration.GetSection("appSettings:ApiUrl").Value + "";
string tokenKey = builder.Configuration.GetSection("appSettings:TokenKey").Value + "";
string tokenValue = builder.Configuration.GetSection("appSettings:TokenValue").Value + "";
// Add services to the container.
builder.Services.AddRazorComponents(options => 
    options.DetailedErrors = builder.Environment.IsDevelopment())
    .AddInteractiveServerComponents();

builder.Services.InstallerExtensionsInAssembly(builder.Configuration);

builder.Services.AddResponseCompression();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
builder.Services.AddHttpClient("api", m =>
{
    m.BaseAddress = new Uri(apiUrl);
    m.Timeout = TimeSpan.FromMinutes(5);
    m.DefaultRequestHeaders.Add(tokenKey, tokenValue);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseResponseCompression();
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.UseStatusCodePagesWithRedirects("/404"); // hainguyen 2023 khi không tìm thấy page nào -> redirect sang page not found

app.Run();
