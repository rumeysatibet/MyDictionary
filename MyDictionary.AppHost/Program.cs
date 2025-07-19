var builder = DistributedApplication.CreateBuilder(args);

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
