var builder = DistributedApplication.CreateBuilder(args);

// Aspire Dashboard port çakışması için alternatif port kullan
Environment.SetEnvironmentVariable("DOTNET_DASHBOARD_OTLP_ENDPOINT_URL", "http://localhost:18889");
Environment.SetEnvironmentVariable("ASPIRE_DASHBOARD_URL", "http://localhost:15070");

// SQL Server container kullan (önerilen)
var sqlServer = builder.AddSqlServer("sqlserver")
    .WithDataVolume(); // Verileri kalıcı tut

var database = sqlServer.AddDatabase("mydictionarydb");

var apiService = builder.AddProject<Projects.MyDictionary_ApiService>("apiservice")
    .WithReference(database)
    .WaitFor(database);

builder.AddProject<Projects.MyDictionary_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
