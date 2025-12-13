namespace FileStorageService.Infrastructure.Data.Dtos;

public sealed class WorkDto
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid AssignmentId { get; set; }

    public DateTime CreatedAt { get; set; }
    
    public string FilePath { get; set; } = null!;
    
    public string? Hash { get; set; }
}