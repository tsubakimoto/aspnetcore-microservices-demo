using TodoApp.Shared.DTOs;
using TodoApp.Shared.DTOs.Task;

namespace TodoApp.Services.Task.Services;

/// <summary>
/// タスクサービスのインターフェース
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// タスク一覧取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="page">ページ番号</param>
    /// <param name="pageSize">ページサイズ</param>
    /// <param name="status">ステータスフィルター</param>
    /// <param name="search">検索文字列</param>
    /// <param name="sortBy">ソート項目</param>
    /// <param name="sortOrder">ソート順</param>
    /// <param name="labelIds">ラベルIDフィルター</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>ページング結果</returns>
    Task<PagedResultDto<TaskDto>> GetTasksAsync(
        string userId,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? search = null,
        string sortBy = "createdAt",
        string sortOrder = "desc",
        IEnumerable<Guid>? labelIds = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// タスク詳細取得
    /// </summary>
    /// <param name="id">タスクID</param>
    /// <param name="userId">ユーザーID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>タスク詳細</returns>
    Task<TaskDto?> GetTaskByIdAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// タスク作成
    /// </summary>
    /// <param name="request">作成リクエスト</param>
    /// <param name="userId">ユーザーID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>作成されたタスク</returns>
    Task<TaskDto> CreateTaskAsync(CreateTaskRequestDto request, string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// タスク更新
    /// </summary>
    /// <param name="id">タスクID</param>
    /// <param name="request">更新リクエスト</param>
    /// <param name="userId">ユーザーID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>更新されたタスク</returns>
    Task<TaskDto?> UpdateTaskAsync(Guid id, UpdateTaskRequestDto request, string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// タスク削除（論理削除）
    /// </summary>
    /// <param name="id">タスクID</param>
    /// <param name="userId">ユーザーID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>削除成功フラグ</returns>
    Task<bool> DeleteTaskAsync(Guid id, string userId, CancellationToken cancellationToken = default);
}