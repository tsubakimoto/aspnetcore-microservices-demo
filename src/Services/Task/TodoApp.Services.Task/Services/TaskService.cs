using Microsoft.EntityFrameworkCore;
using TodoApp.Services.Task.Data;
using TodoApp.Services.Task.Models;
using TodoApp.Shared.Common;
using TodoApp.Shared.DTOs;
using TodoApp.Shared.DTOs.Task;
using TodoApp.Shared.DTOs.Label;
using TodoApp.Shared.DTOs.File;
using TodoApp.Shared.Models;

namespace TodoApp.Services.Task.Services;

/// <summary>
/// タスクサービスの実装
/// </summary>
public class TaskService : ITaskService
{
    private readonly TaskDbContext _context;
    private readonly ILogger<TaskService> _logger;

    public TaskService(TaskDbContext context, ILogger<TaskService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResultDto<TaskDto>> GetTasksAsync(
        string userId,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? search = null,
        string sortBy = "createdAt",
        string sortOrder = "desc",
        IEnumerable<Guid>? labelIds = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Tasks
                .Where(t => t.UserId == userId && !t.IsDeleted);

            // ステータスフィルター
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<TodoTaskStatus>(status, true, out var statusEnum))
            {
                query = query.Where(t => t.Status == statusEnum);
            }

            // 検索フィルター
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Title.Contains(search) || 
                                    (t.Description != null && t.Description.Contains(search)));
            }

            // ラベルフィルター
            if (labelIds != null && labelIds.Any())
            {
                var labelIdList = labelIds.ToList();
                query = query.Where(t => t.TaskLabels.Any(tl => labelIdList.Contains(tl.LabelId)));
            }

            // ソート
            query = sortBy.ToLower() switch
            {
                "title" => sortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(t => t.Title)
                    : query.OrderBy(t => t.Title),
                "duedate" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(t => t.DueDate)
                    : query.OrderBy(t => t.DueDate),
                "updatedat" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(t => t.UpdatedAt)
                    : query.OrderBy(t => t.UpdatedAt),
                "priority" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(t => t.Priority)
                    : query.OrderBy(t => t.Priority),
                _ => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(t => t.CreatedAt)
                    : query.OrderBy(t => t.CreatedAt),
            };

            // 総件数取得
            var totalCount = await query.CountAsync(cancellationToken);

            // ページング計算
            var (skip, take, totalPages, hasNext, hasPrevious) = PaginationHelper.Calculate(totalCount, page, pageSize);

            // データ取得
            var tasks = await query
                .Skip(skip)
                .Take(take)
                .Include(t => t.TaskLabels)
                .Include(t => t.TaskFiles.Where(f => !f.IsDeleted))
                .ToListAsync(cancellationToken);

            // DTOに変換
            var taskDtos = tasks.Select(MapToTaskDto).ToList();

            return new PagedResultDto<TaskDto>
            {
                Data = taskDtos,
                Pagination = new PaginationDto
                {
                    CurrentPage = page,
                    PageSize = take,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasNext = hasNext,
                    HasPrevious = hasPrevious
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "タスク一覧取得でエラーが発生しました。UserId: {UserId}", userId);
            throw;
        }
    }

    public async Task<TaskDto?> GetTaskByIdAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var task = await _context.Tasks
                .Include(t => t.TaskLabels)
                .Include(t => t.TaskFiles.Where(f => !f.IsDeleted))
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && !t.IsDeleted, cancellationToken);

            return task != null ? MapToTaskDto(task) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "タスク詳細取得でエラーが発生しました。TaskId: {TaskId}, UserId: {UserId}", id, userId);
            throw;
        }
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskRequestDto request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var task = new TaskEntity
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                Status = TodoTaskStatus.Pending,
                Priority = request.Priority,
                DueDate = request.DueDate,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);

            // ラベルの関連付け
            if (request.LabelIds.Any())
            {
                foreach (var labelId in request.LabelIds)
                {
                    var taskLabel = new TaskLabelEntity
                    {
                        TaskId = task.Id,
                        LabelId = labelId,
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.TaskLabels.Add(taskLabel);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("タスクが作成されました。TaskId: {TaskId}, UserId: {UserId}", task.Id, userId);

            // 作成されたタスクを取得して返却
            return await GetTaskByIdAsync(task.Id, userId, cancellationToken) 
                ?? throw new InvalidOperationException("作成されたタスクが見つかりません");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "タスク作成でエラーが発生しました。UserId: {UserId}", userId);
            throw;
        }
    }

    public async Task<TaskDto?> UpdateTaskAsync(Guid id, UpdateTaskRequestDto request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var task = await _context.Tasks
                .Include(t => t.TaskLabels)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && !t.IsDeleted, cancellationToken);

            if (task == null)
            {
                return null;
            }

            // 基本情報更新
            task.Title = request.Title;
            task.Description = request.Description;
            task.Status = request.Status;
            task.Priority = request.Priority;
            task.DueDate = request.DueDate;

            // ラベルの更新
            // 既存のラベル関連を削除
            _context.TaskLabels.RemoveRange(task.TaskLabels);

            // 新しいラベル関連を追加
            if (request.LabelIds.Any())
            {
                foreach (var labelId in request.LabelIds)
                {
                    var taskLabel = new TaskLabelEntity
                    {
                        TaskId = task.Id,
                        LabelId = labelId,
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.TaskLabels.Add(taskLabel);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("タスクが更新されました。TaskId: {TaskId}, UserId: {UserId}", id, userId);

            return await GetTaskByIdAsync(id, userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "タスク更新でエラーが発生しました。TaskId: {TaskId}, UserId: {UserId}", id, userId);
            throw;
        }
    }

    public async Task<bool> DeleteTaskAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && !t.IsDeleted, cancellationToken);

            if (task == null)
            {
                return false;
            }

            // 論理削除
            task.IsDeleted = true;
            task.DeletedAt = DateTime.UtcNow;
            task.Status = TodoTaskStatus.Deleted;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("タスクが削除されました。TaskId: {TaskId}, UserId: {UserId}", id, userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "タスク削除でエラーが発生しました。TaskId: {TaskId}, UserId: {UserId}", id, userId);
            throw;
        }
    }

    private static TaskDto MapToTaskDto(TaskEntity task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            CompletedAt = task.CompletedAt,
            Labels = task.TaskLabels.Select(tl => new LabelDto
            {
                Id = tl.LabelId,
                Name = "ラベル", // TODO: ラベルサービスから取得
                Color = "#6366F1"
            }).ToList(),
            Files = task.TaskFiles.Where(f => !f.IsDeleted).Select(f => new TaskFileDto
            {
                Id = f.Id,
                FileName = f.OriginalFileName,
                FileSize = f.FileSize,
                ContentType = f.ContentType,
                BlobUrl = $"{f.BlobContainerName}/{f.BlobPath}", // TODO: 実際のBlobURL生成
                UploadedAt = f.UploadedAt
            }).ToList()
        };
    }
}