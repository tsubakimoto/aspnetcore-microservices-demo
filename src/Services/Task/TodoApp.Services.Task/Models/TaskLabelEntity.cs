namespace TodoApp.Services.Task.Models;

/// <summary>
/// タスクラベル関連エンティティ
/// </summary>
public class TaskLabelEntity
{
    /// <summary>
    /// タスクID
    /// </summary>
    public Guid TaskId { get; set; }
    
    /// <summary>
    /// ラベルID
    /// </summary>
    public Guid LabelId { get; set; }
    
    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 作成者ID
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// タスクエンティティ（ナビゲーションプロパティ）
    /// </summary>
    public virtual TaskEntity? Task { get; set; }
}