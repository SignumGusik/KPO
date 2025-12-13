
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FileStorageService.Infrastructure.Data;

internal sealed class DesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<FileStorageDbContext>
{
    public FileStorageDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<FileStorageDbContext>()
            .UseNpgsql("Server=127.0.0.1;Port=5432;Database=filestorage;User Id=postgres;Password=postgres;")
            .Options;

        return new FileStorageDbContext(options);
    }
}