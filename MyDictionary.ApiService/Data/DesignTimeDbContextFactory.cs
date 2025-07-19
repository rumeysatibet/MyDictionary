using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MyDictionary.ApiService.Data;

// This factory is used by the 'dotnet ef' command-line tools to create a DbContext instance
// at design time. It provides a way to configure the DbContext with a connection string
// for migrations, without needing the full application host to be running.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DictionaryDbContext>
{
    public DictionaryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DictionaryDbContext>();
        // This is a hard-coded connection string for design-time tools.
        // The actual connection string for the running application will be injected by Aspire.
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=mydictionarydb;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new DictionaryDbContext(optionsBuilder.Options);
    }
}
