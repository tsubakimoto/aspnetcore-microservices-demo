namespace TodoApp.Shared.DTOs.Label;

/// <summary>
/// ラベル更新リクエストDTO
/// </summary>
public class UpdateLabelRequestDto
{
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
}