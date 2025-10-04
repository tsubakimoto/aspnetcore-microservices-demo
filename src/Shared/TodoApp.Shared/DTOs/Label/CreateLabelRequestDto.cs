namespace TodoApp.Shared.DTOs.Label;

/// <summary>
/// ラベル作成リクエストDTO
/// </summary>
public class CreateLabelRequestDto
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
}