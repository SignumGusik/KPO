using FileStorageService.Entities.Models;

namespace FileStorageService.UseCases.Works.UploadWork;

/// <summary>
/// Обработчик команды загрузки работы.
/// Валидация входных данных, создание доменной сущности Work,
/// вызов репозитория для записи файла и метаданных, возврат UploadWorkResponse.
/// </summary>
internal sealed class UploadWorkRequestHandler : IUploadWorkRequestHandler
{
    private readonly IUploadWorkRepository _repository;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Конструктор получает репозиторий для сохранения и поставщик времени.
    /// </summary>
    public UploadWorkRequestHandler(
        IUploadWorkRepository repository,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Обрабатывает команду UploadWorkCommand:
    /// - валидирует параметры,
    /// - создаёт доменную сущность Work с placeholder filePath,
    /// - вызывает репозиторий для сохранения файла и метаданных,
    /// - возвращает UploadWorkResponse с workId.
    /// </summary>
    public UploadWorkResponse Handle(UploadWorkCommand command)
    {
        if (command.StudentId == Guid.Empty)
            throw new ArgumentException("StudentId is required", nameof(command.StudentId));

        if (command.AssignmentId == Guid.Empty)
            throw new ArgumentException("AssignmentId is required", nameof(command.AssignmentId));

        if (string.IsNullOrWhiteSpace(command.FileName))
            throw new ArgumentException("FileName is required", nameof(command.FileName));

       
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var work = new Work(
            id: Guid.NewGuid(),
            studentId: command.StudentId,
            assignmentId: command.AssignmentId,
            createdAt: now,
            filePath: "placeholder" 
        );

        var savedWork = _repository.Save(work, command.Content, command.FileName);

        return new UploadWorkResponse(savedWork.Id);
    }
}