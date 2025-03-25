using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CustomTemplateSearch.EntityFrameworkCore;

public class CustomDbContext : DbContext
{
    private IHostEnvironment _environment;

    public DbSet<CatalogEntity> Catalog { get; set; }

    public DbSet<Users> Users { get; set; }

    public DbSet<RoleEntity> Roles { get; set; }

    public DbSet<Template> Templates { get; set; }

    public DbSet<TemplateColumn> TemplateColumns { get; set; }

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