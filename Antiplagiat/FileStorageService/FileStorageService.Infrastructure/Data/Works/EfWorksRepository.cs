using System.IO;
using System.Security.Cryptography;
using FileStorageService.Entities.Models;
using FileStorageService.Infrastructure.Data.Dtos;
using FileStorageService.UseCases.Works.GetWork;
using FileStorageService.UseCases.Works.UploadWork;
using Microsoft.EntityFrameworkCore;

namespace FileStorageService.Infrastructure.Data.Works;
/// <summary>
/// Репозиторий работ с реализацией на EF Core и файловой системе.
/// Сохраняет файл в каталоге и метаданные в таблице Works.
/// </summary>
internal sealed class EfWorksRepository : IUploadWorkRepository, IGetWorkRepository
{
    private readonly FileStorageDbContext _dbContext;
    private readonly string _filesDir;

    /// <summary>
    /// Конструктор: подготавливает директорию для файлов (создаёт, если не существует).
    /// </summary>
    public EfWorksRepository(FileStorageDbContext dbContext)
    {
        _dbContext = dbContext;
        
        var root = Directory.GetCurrentDirectory();
        _filesDir = Path.Combine(root, "files");
        Directory.CreateDirectory(_filesDir);
    }

    /// <summary>
    /// Сохраняет файл на диск и метаданные в БД. Возвращает финальную доменную сущность Work с реальным путём и хэшем.
    /// </summary>
    public Work Save(Work work, Stream fileStream, string fileName)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".bin";
        }

        var fileNameOnDisk = $"{work.Id}{extension}";
        var fullPath = Path.Combine(_filesDir, fileNameOnDisk);
        
        byte[] fileBytes;
        using (var memory = new MemoryStream())
        {
            fileStream.CopyTo(memory);
            fileBytes = memory.ToArray();
        }
        
        string fileHash;
        using (var sha = SHA256.Create())
        {
            var hashBytes = sha.ComputeHash(fileBytes);
            fileHash = Convert.ToHexString(hashBytes); 
        }
        using (var file = File.Create(fullPath))
        {
            file.Write(fileBytes, 0, fileBytes.Length);
        }

        var finalWork = new Work(
            work.Id,
            work.StudentId,
            work.AssignmentId,
            work.CreatedAt,
            fullPath,
            fileHash
        );

        var dto = finalWork.ToDto();

        _dbContext.Works.Add(dto);
        _dbContext.SaveChanges();

        return finalWork;
    }

    /// <summary>
    /// Получает работу по id (если есть) и возвращает доменную сущность Work.
    /// </summary>
    public Work? GetById(Guid id)
    {
        var dto = _dbContext.Works
            .AsNoTracking()
            .SingleOrDefault(w => w.Id == id);

        return dto?.ToEntity();
    }
}