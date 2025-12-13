using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FileAnalysisService.Entities.Models;

namespace FileAnalysisService.UseCases.RunAnalysis;

public interface IReportsRepository
{
    /// <summary>
    /// Найти все отчёты по этому же заданию, но других студентов.
    /// </summary>
    Task<IReadOnlyList<Report>> GetPreviousReportsForAssignmentAsync(
        Guid assignmentId,
        Guid excludeStudentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохранить отчёт.
    /// </summary>
    Task<Report> AddAsync(Report report, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все отчёты по конкретной работе.
    /// </summary>
    Task<IReadOnlyList<Report>> GetByWorkIdAsync(
        Guid workId,
        CancellationToken cancellationToken = default);
}