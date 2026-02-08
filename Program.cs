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

// ✅ FIXED: Use IHttpClientFactory for proper HTTP client management
builder.Services.AddHttpClient();

var app = builder.Build();


// =======================================================
// ✅ FIXED: Removed global latency middleware
// Previous issue: Every request delayed by 5 seconds
// =======================================================
// Performance issue fixed: Removed blocking 5-second delay


// =======================================================
// ✅ FIXED: Removed manual HttpClient instantiation
// Now using IHttpClientFactory via dependency injection
// =======================================================


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
// ✅ FIXED: Healthcheck endpoint
// - Now properly async (no blocking)
// - Uses IHttpClientFactory
// - Removed unnecessary delays
// - Proper error handling with logging
// =======================================================
app.MapGet("/healthcheck", async (IHttpClientFactory httpClientFactory, ILogger<Program> logger) =>
{
    try
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(5);
        
        var response = await httpClient.GetAsync("https://www.google.com");
        
        if (response.IsSuccessStatusCode)
        {
            return Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
        }
        else
        {
            logger.LogWarning("Healthcheck: External dependency returned status {StatusCode}", response.StatusCode);
            return Results.Ok(new { status = "Degraded", timestamp = DateTime.UtcNow });
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Healthcheck: Failed to reach external dependency");
        return Results.Ok(new { status = "Degraded", error = "External dependency unreachable", timestamp = DateTime.UtcNow });
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
