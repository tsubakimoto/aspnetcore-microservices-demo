namespace TodoApp.Shared.DTOs.File;

/// <summary>
/// タスクファイルレスポンスDTO
/// </summary>
public class TaskFileDto
{
    /// <summary>
    /// ファイルID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// オリジナルファイル名
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// ファイルサイズ（バイト）
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// コンテンツタイプ
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    
    /// <summary>
    /// Blob URL
    /// </summary>
    public string? BlobUrl { get; set; }
    
    /// <summary>
    /// アップロード日時
    /// </summary>
    public DateTime UploadedAt { get; set; }
}