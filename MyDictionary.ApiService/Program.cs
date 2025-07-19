using Microsoft.EntityFrameworkCore;
using MyDictionary.ApiService.Data;
using MyDictionary.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Add controllers
builder.Services.AddControllers();

// CORS ekle
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.AddSqlServerDbContext<DictionaryDbContext>("mydictionarydb");


// Auth service
builder.Services.AddScoped<IAuthService, AuthService>();

// Data seeding service
builder.Services.AddScoped<DataSeedingService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// CORS'u aktif et
app.UseCors("AllowAll");

// OpenAPI'yi her zaman aktif et (Aspire için)
app.MapOpenApi();

// Swagger UI ekle
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "MyDictionary API V1");
        options.RoutePrefix = string.Empty; // Root path'te açılsın
    });
}

app.MapGet("/topics", async (DictionaryDbContext db) =>
{
    return await db.Topics.ToListAsync();
});

// Map controllers
app.MapControllers();

app.MapDefaultEndpoints();

// Migration ve data seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DictionaryDbContext>();
    
    // Migration'ları otomatik uygula (Aspire container için)
    await context.Database.MigrateAsync();
    
    var seedingService = scope.ServiceProvider.GetRequiredService<DataSeedingService>();
    await seedingService.SeedDataAsync();
}

app.Run();

