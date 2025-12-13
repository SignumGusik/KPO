using FileAnalysisService.Infrastructure.Data.Dtos;
using Microsoft.EntityFrameworkCore;

namespace FileAnalysisService.Infrastructure.Data;

/// <summary>
/// EF Core DbContext для FileAnalysisService.
/// Содержит DbSet<ReportDto> и конфигурацию таблицы Reports в OnModelCreating.
/// </summary>
public sealed class FileAnalysisDbContext(
    DbContextOptions<FileAnalysisDbContext> options
) : DbContext(options)
{
    /// <summary>Набор DTO отчётов.</summary>
    public DbSet<ReportDto> Reports { get; set; } = null!;

    /// <summary>
    /// Конфигурация модели EF Core: название таблицы, ключи и ограничения колонок.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReportDto>(builder =>
        {
            builder.ToTable("Reports");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.WorkId).IsRequired();
            builder.Property(r => r.StudentId).IsRequired();
            builder.Property(r => r.AssignmentId).IsRequired();
            builder.Property(r => r.Status).IsRequired();
            builder.Property(r => r.Score).IsRequired();
            builder.Property(r => r.CreatedAt).IsRequired();

            builder.Property(r => r.WordCloudUrl)
                .HasMaxLength(1024);
            
            builder.Property(r => r.DocumentHash)
                .HasMaxLength(64);
        });
    }
}