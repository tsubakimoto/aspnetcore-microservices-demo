namespace TodoApp.Shared.Constants;

/// <summary>
/// ファイル関連の定数
/// </summary>
public static class FileConstants
{
    /// <summary>
    /// 最大ファイルサイズ（10MB）
    /// </summary>
    public const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    
    /// <summary>
    /// 許可されるファイル拡張子
    /// </summary>
    public static readonly string[] AllowedExtensions = 
    {
        ".jpg", ".jpeg", ".png", ".gif", // 画像
        ".pdf", // PDF
        ".doc", ".docx", // Word
        ".xls", ".xlsx", // Excel
        ".txt" // テキスト
    };
    
    /// <summary>
    /// 許可されるコンテンツタイプ
    /// </summary>
    public static readonly string[] AllowedContentTypes = 
    {
        "image/jpeg",
        "image/png", 
        "image/gif",
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "text/plain"
    };
    
    /// <summary>
    /// Blobコンテナ名
    /// </summary>
    public const string TaskFilesBlobContainer = "task-files";
}