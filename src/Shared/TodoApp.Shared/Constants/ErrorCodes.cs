namespace TodoApp.Shared.Constants;

/// <summary>
/// エラーコード定数
/// </summary>
public static class ErrorCodes
{
    /// <summary>
    /// バリデーションエラー
    /// </summary>
    public const string ValidationError = "VALIDATION_ERROR";
    
    /// <summary>
    /// 認証エラー
    /// </summary>
    public const string Unauthorized = "UNAUTHORIZED";
    
    /// <summary>
    /// 認可エラー
    /// </summary>
    public const string Forbidden = "FORBIDDEN";
    
    /// <summary>
    /// リソースが見つからない
    /// </summary>
    public const string NotFound = "NOT_FOUND";
    
    /// <summary>
    /// リソースの競合
    /// </summary>
    public const string Conflict = "CONFLICT";
    
    /// <summary>
    /// ビジネスルール違反
    /// </summary>
    public const string UnprocessableEntity = "UNPROCESSABLE_ENTITY";
    
    /// <summary>
    /// 内部サーバーエラー
    /// </summary>
    public const string InternalError = "INTERNAL_ERROR";
    
    /// <summary>
    /// サービス利用不可
    /// </summary>
    public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
    
    /// <summary>
    /// ペイロードサイズ超過
    /// </summary>
    public const string PayloadTooLarge = "PAYLOAD_TOO_LARGE";
}