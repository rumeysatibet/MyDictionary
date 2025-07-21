using McpProbe.Blazor.Wasm.Extensions;
using MyDictionary.Web.Components;
using MyDictionary.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMcpProbeWasm(builder.Configuration);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// SignalR timeout ayarları
builder.Services.AddSignalR(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

builder.Services.AddOutputCache();

// Authentication State Service ekle
builder.Services.AddScoped<AuthenticationStateService>();

// API servisine HttpClient ekle - Aspire service discovery
builder.Services.AddHttpClient("ApiService", client =>
{
    // Aspire service discovery otomatik olarak URL'i çözecek
});

// Default HttpClient için base URL config (fallback için)
builder.Services.AddHttpClient(string.Empty, client =>
{
    client.BaseAddress = new Uri("https://apiservice/");
});



var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Uploads proxy - API service'deki static file'lara erişim için
app.Map("/uploads/{**path}", async (HttpContext context, IHttpClientFactory httpClientFactory, string path) =>
{
    try
    {
        var httpClient = httpClientFactory.CreateClient("ApiService");
        var response = await httpClient.GetAsync($"https://apiservice/uploads/{path}");
        
        if (response.IsSuccessStatusCode)
        {
            context.Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            var content = await response.Content.ReadAsByteArrayAsync();
            await context.Response.Body.WriteAsync(content);
        }
        else
        {
            context.Response.StatusCode = (int)response.StatusCode;
        }
    }
    catch (Exception ex)
    {
        // Log the exception and return 404
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error proxying uploads request for path: {Path}", path);
        context.Response.StatusCode = 404;
    }
});

app.MapDefaultEndpoints();

app.Run();
