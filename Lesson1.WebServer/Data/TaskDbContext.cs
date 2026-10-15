using Lesson1.WebServer.Services;
using Microsoft.EntityFrameworkCore;

namespace Lesson1.WebServer.Data;

public sealed class TaskDbContext(DbContextOptions<TaskDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(50);

            // Связь один-ко-многим: Category -> TaskItem
            entity.HasMany(c => c.Tasks)
                .WithOne(t => t.Category)
                .HasForeignKey(t => t.CategoryId)
                // При удалении категории задачи не удаляются, CategoryId = null
                .OnDelete(DeleteBehavior.SetNull);

            // Начальное наполнение категорий
            entity.HasData(
                new Category { Id = 1, Name = "Работа" },
                new Category { Id = 2, Name = "Учёба" },
                new Category { Id = 3, Name = "Личное" });
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(t => t.CreatedAt)
                .HasDefaultValueSql("datetime('now')");
        });

        base.OnModelCreating(modelBuilder);
    }
}