using System.ComponentModel.DataAnnotations;
using TodoApp.Shared.Models;

namespace TodoApp.Shared.DTOs.Task;

/// <summary>
/// タスク作成リクエストDTO
/// </summary>
public class CreateTaskRequestDto
{
    /// <summary>
    /// タイトル
    /// </summary>
    [Required(ErrorMessage = "タイトルは必須です")]
    [StringLength(200, ErrorMessage = "タイトルは200文字以内で入力してください")]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 詳細内容
    /// </summary>
    [StringLength(2000, ErrorMessage = "詳細内容は2000文字以内で入力してください")]
    public string? Description { get; set; }
    
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