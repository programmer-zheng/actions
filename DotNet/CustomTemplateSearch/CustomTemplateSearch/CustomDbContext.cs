using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CustomTemplateSearch;

public class CustomDbContext : DbContext
{
    private IHostEnvironment _environment;

    public DbSet<CatalogEntity> Catalog { get; set; }


    public CustomDbContext(DbContextOptions<CustomDbContext> options, IHostEnvironment environment)
        : base(options)
    {
        _environment = environment;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var sourcePath = Path.Combine(_environment.ContentRootPath, "Catalog.json");
        var sourceJson = File.ReadAllText(sourcePath);
        var sourceItems = JsonConvert.DeserializeObject<CatalogEntity[]>(sourceJson);

        modelBuilder.Entity<CatalogEntity>().HasData(sourceItems);
    }
}

public class CatalogEntity
{
    public int Id { get; set; }
    public string Type { get; set; }
    public string Brand { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    [Precision(18, 2)]
    public decimal Price { get; set; }
}