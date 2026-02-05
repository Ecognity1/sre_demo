// using MyFirstAzureWebApp.Components;

// var builder = WebApplication.CreateBuilder(args);

// // Add services to the container.
// builder.Services.AddRazorComponents()
//     .AddInteractiveServerComponents();

// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Error", createScopeForErrors: true);
//     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//     app.UseHsts();
// }

// app.UseHttpsRedirection();

// app.UseStaticFiles();
// app.UseAntiforgery();

// app.MapRazorComponents<App>()
//     .AddInteractiveServerRenderMode();

// app.Run();

using MyFirstAzureWebApp.Components;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// =======================================================
// ❌ SECURITY ISSUE — Hardcoded secret in source code
// SRE/Security tools should flag this immediately
// =======================================================
string connectionString =
    "Server=myserver.database.windows.net;User Id=admin;Password=Admin123;";


// Add services to container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();


// =======================================================
// ❌ PERFORMANCE ISSUE — Global latency middleware
// Every request delayed by 5 seconds
// Site will feel VERY slow
// =======================================================
app.Use(async (context, next) =>
{
    Thread.Sleep(5000); // blocking thread (very bad practice)
    await next();
});


// =======================================================
// ❌ RESOURCE MISUSE — HttpClient created manually
// Should use IHttpClientFactory instead
// Can cause socket exhaustion
// =======================================================
var httpClient = new HttpClient();


// =======================================================
// Normal pipeline
// =======================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();


// =======================================================
// ❌ MULTIPLE PROBLEMS ENDPOINT
// - Blocking async (.Result)
// - Swallowed exceptions
// - Extra sleep
// - No logging
// =======================================================
app.MapGet("/healthcheck", () =>
{
    try
    {
        // BAD: blocks thread pool
        var result = httpClient
            .GetAsync("https://www.google.com")
            .Result;

        // BAD: more delay
        Thread.Sleep(3000);

        return Results.Ok("Healthy");
    }
    catch (Exception)
    {
        // BAD: completely ignored
        return Results.Ok("Still Healthy");
    }
});


// =======================================================
// ❌ CPU SPIKE DEMO ENDPOINT
// Simulates high CPU usage
// =======================================================
app.MapGet("/cpu", () =>
{
    while (true)
    {
        // infinite loop -> CPU spike
    }
});


// =======================================================
// ❌ MEMORY PRESSURE DEMO ENDPOINT
// Allocates large memory
// =======================================================
app.MapGet("/memory", () =>
{
    var bigList = new List<byte[]>();

    for (int i = 0; i < 100; i++)
    {
        bigList.Add(new byte[10_000_000]); // ~10MB each
    }

    return Results.Ok("Allocated lots of memory");
});


// Normal Razor components
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
