using TodoApp.Shared.Models;
using TodoApp.Shared.DTOs.Label;
using TodoApp.Shared.DTOs.File;

namespace TodoApp.Shared.DTOs.Task;

/// <summary>
/// タスク詳細レスポンスDTO
/// </summary>
public class TaskDto
{
    /// <summary>
    /// タスクID
    /// </summary>
    public Guid Id { get; set; }
    
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
    /// 優先度
    /// </summary>
    public Priority Priority { get; set; }
    
    /// <summary>
    /// 期日
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// 完了日時
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// ラベルのリスト
    /// </summary>
    public IEnumerable<LabelDto> Labels { get; set; } = new List<LabelDto>();
    
    /// <summary>
    /// ファイルのリスト
    /// </summary>
    public IEnumerable<TaskFileDto> Files { get; set; } = new List<TaskFileDto>();
}