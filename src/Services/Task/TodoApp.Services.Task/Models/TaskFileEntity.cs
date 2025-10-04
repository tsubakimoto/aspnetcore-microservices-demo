namespace TodoApp.Services.Task.Models;

/// <summary>
/// タスクファイルエンティティ
/// </summary>
public class TaskFileEntity
{
    /// <summary>
    /// ファイルID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// タスクID
    /// </summary>
    public Guid TaskId { get; set; }
    
    /// <summary>
    /// オリジナルファイル名
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;
    
    /// <summary>
    /// 保存されたファイル名
    /// </summary>
    public string StoredFileName { get; set; } = string.Empty;
    
    /// <summary>
    /// ファイルサイズ
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// コンテンツタイプ
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    
    /// <summary>
    /// Blobコンテナ名
    /// </summary>
    public string BlobContainerName { get; set; } = "task-files";
    
    /// <summary>
    /// Blobパス
    /// </summary>
    public string BlobPath { get; set; } = string.Empty;
    
    /// <summary>
    /// アップロード日時
    /// </summary>
    public DateTime UploadedAt { get; set; }
    
    /// <summary>
    /// アップロード者ID
    /// </summary>
    public string UploadedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// 削除フラグ
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// 削除日時
    /// </summary>
    public DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// チェックサム
    /// </summary>
    public string? Checksum { get; set; }
    
    /// <summary>
    /// タスクエンティティ（ナビゲーションプロパティ）
    /// </summary>
    public virtual TaskEntity? Task { get; set; }
}