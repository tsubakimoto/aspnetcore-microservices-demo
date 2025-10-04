using TodoApp.Shared.Models;

namespace TodoApp.Services.Task.Models;

/// <summary>
/// タスクエンティティ
/// </summary>
public class TaskEntity
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
    /// 削除フラグ
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// 削除日時
    /// </summary>
    public DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// ユーザーID（Azure AD）
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// バージョン（楽観的ロック）
    /// </summary>
    public byte[] Version { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// タスクラベルのリスト
    /// </summary>
    public virtual ICollection<TaskLabelEntity> TaskLabels { get; set; } = new List<TaskLabelEntity>();
    
    /// <summary>
    /// タスクファイルのリスト
    /// </summary>
    public virtual ICollection<TaskFileEntity> TaskFiles { get; set; } = new List<TaskFileEntity>();
}