namespace TodoApp.Shared.DTOs.Label;

/// <summary>
/// ラベルレスポンスDTO
/// </summary>
public class LabelDto
{
    /// <summary>
    /// ラベルID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// ラベル名
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 色（HEX形式）
    /// </summary>
    public string Color { get; set; } = "#6366F1";
    
    /// <summary>
    /// 説明
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 表示順序
    /// </summary>
    public int SortOrder { get; set; }
    
    /// <summary>
    /// タスク数
    /// </summary>
    public int TaskCount { get; set; }
    
    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}