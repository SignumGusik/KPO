using FileStorageService.Entities.Models;

namespace FileStorageService.UseCases.Works.GetWork;

public interface IGetWorkRepository
{
    Work? GetById(Guid id);
}