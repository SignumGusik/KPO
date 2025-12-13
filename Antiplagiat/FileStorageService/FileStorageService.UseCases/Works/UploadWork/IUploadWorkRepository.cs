using System.IO;
using FileStorageService.Entities.Models;

namespace FileStorageService.UseCases.Works.UploadWork;

public interface IUploadWorkRepository
{
    /// <summary>
    /// Сохраняет файл на диск + метаданные в БД.
    /// Возвращает сохранённую доменную сущность.
    /// </summary>
    Work Save(Work work, Stream fileStream, string fileName);
}