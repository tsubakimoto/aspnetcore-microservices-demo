namespace TodoApp.Shared.DTOs;

/// <summary>
/// エラー情報を表すDTO
/// </summary>
public class ErrorDto
{
    /// <summary>
    /// エラーコード
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// エラー詳細のリスト
    /// </summary>
    public IEnumerable<ErrorDetailDto> Details { get; set; } = new List<ErrorDetailDto>();
    
    /// <summary>
    /// タイムスタンプ
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// リクエストID
    /// </summary>
    public string? RequestId { get; set; }
    
    /// <summary>
    /// リクエストパス
    /// </summary>
    public string? Path { get; set; }
}

/// <summary>
/// エラー詳細を表すDTO
/// </summary>
public class ErrorDetailDto
{
    /// <summary>
    /// フィールド名
    /// </summary>
    public string Field { get; set; } = string.Empty;
    
    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public string Message { get; set; } = string.Empty;
}