using Microsoft.EntityFrameworkCore;

namespace Data;

public class DataContext : DbContext
{
    private readonly bool _useDesign;

    public DataContext()
    {
        _useDesign = true;
    }

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_useDesign)
        {
            optionsBuilder.UseSqlite("Data Source=data.db");
        }
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new FormDataConfiguration());
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<FormData> Forms { get; set; }
}