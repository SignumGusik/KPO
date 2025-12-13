
using FileStorageService.Infrastructure.Data.Dtos;
using Microsoft.EntityFrameworkCore;

namespace FileStorageService.Infrastructure.Data;

public sealed class FileStorageDbContext(DbContextOptions<FileStorageDbContext> options)
    : DbContext(options)
{
    public DbSet<WorkDto> Works { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkDto>(builder =>
        {
            builder.HasKey(w => w.Id);
            builder.ToTable("Works");

            builder.Property(w => w.StudentId).IsRequired();
            builder.Property(w => w.AssignmentId).IsRequired();
            builder.Property(w => w.CreatedAt).IsRequired();
            builder.Property(w => w.FilePath).IsRequired();
            builder.Property(w => w.Hash).HasMaxLength(256);
        });
    }
}