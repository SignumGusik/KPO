using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileAnalysisService.Entities.Models;
using FileAnalysisService.UseCases.RunAnalysis;
using Microsoft.EntityFrameworkCore;

namespace FileAnalysisService.Infrastructure.Data.Reports;
/// <summary>
/// Реализация репозитория отчётов поверх EF Core (FileAnalysisDbContext).
/// Отвечает за добавление отчётов и выборку по workId / assignmentId.
/// </summary>
internal sealed class EfReportsRepository : IReportsRepository
{
    private readonly FileAnalysisDbContext _dbContext;

    /// <summary>
    /// Конструктор получает DbContext через DI.
    /// </summary>
    public EfReportsRepository(FileAnalysisDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Добавляет новый Report в БД и возвращает сохранённую сущность.
    /// </summary>
    public async Task<Report> AddAsync(Report report, CancellationToken cancellationToken = default)
    {
        var dto = report.ToDto();

        _dbContext.Reports.Add(dto);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return dto.ToEntity();
    }

    /// <summary>
    /// Возвращает список Report для указанного workId (отсортированный по createdAt).
    /// </summary>
    public async Task<IReadOnlyList<Report>> GetByWorkIdAsync(
        Guid workId,
        CancellationToken cancellationToken = default)
    {
        var dtos = await _dbContext.Reports
            .AsNoTracking()
            .Where(r => r.WorkId == workId)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return dtos.Select(d => d.ToEntity()).ToList();
    }

    /// <summary>
    /// Возвращает предыдущие отчёты для указанного задания, исключая указанный studentId.
    /// Используется для поиска похожих работ при анализе.
    /// </summary>
    public async Task<IReadOnlyList<Report>> GetPreviousReportsForAssignmentAsync(
        Guid assignmentId,
        Guid excludeStudentId,
        CancellationToken cancellationToken = default)
    {
        var dtos = await _dbContext.Reports
            .AsNoTracking()
            .Where(r => r.AssignmentId == assignmentId && r.StudentId != excludeStudentId)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return dtos.Select(d => d.ToEntity()).ToList();
    }
}