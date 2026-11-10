using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Entities;

namespace TaskManager.Api.Data;

// el esquema lo crea database/script.sql, aqui solo se mapea (sin migraciones)
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.Property(u => u.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<TaskItem>(e =>
        {
            e.Property(t => t.Status).HasConversion<string>();
            e.Property(t => t.Priority).HasComputedColumnSql("JSON_VALUE(AdditionalInfo, '$.priority')");
            e.Property(t => t.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId);
        });
    }
}
