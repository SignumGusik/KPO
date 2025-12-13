namespace FileStorageService.Entities.Models;

public sealed class Work
{
    public Work(
        Guid id,
        Guid studentId,
        Guid assignmentId,
        DateTime createdAt,
        string filePath,
        string? fileHash = null)
    {
        if (studentId == Guid.Empty)
            throw new ArgumentException("StudentId cannot be empty", nameof(studentId));

        if (assignmentId == Guid.Empty)
            throw new ArgumentException("AssignmentId cannot be empty", nameof(assignmentId));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("FilePath cannot be empty", nameof(filePath));

        Id = id;
        StudentId = studentId;
        AssignmentId = assignmentId;
        CreatedAt = createdAt;
        FilePath = filePath;
        FileHash = fileHash;
    }

    public Guid Id { get; }
    public Guid StudentId { get; }
    public Guid AssignmentId { get; }
    public DateTime CreatedAt { get; }

    /// <summary>Абсолютный или относительный путь к файлу на диске.</summary>
    public string FilePath { get; }

    /// <summary>Хэш файла (можно пока не считать, оставить null).</summary>
    public string? FileHash { get; }
}