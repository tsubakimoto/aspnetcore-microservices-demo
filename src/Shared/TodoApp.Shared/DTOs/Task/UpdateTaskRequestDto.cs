using TodoApp.Shared.Models;

namespace TodoApp.Shared.DTOs.Task;

/// <summary>
/// タスク更新リクエストDTO
/// </summary>
public class UpdateTaskRequestDto
{
    /// <summary>
    /// タイトル
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 詳細内容
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// ステータス
    /// </summary>
    public TodoTaskStatus Status { get; set; }
    
    /// <summary>
    /// 期日
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// 優先度
    /// </summary>
    public Priority Priority { get; set; } = Priority.Medium;
    
    /// <summary>
    /// ラベルIDのリスト
    /// </summary>
    public IEnumerable<Guid> LabelIds { get; set; } = new List<Guid>();
}