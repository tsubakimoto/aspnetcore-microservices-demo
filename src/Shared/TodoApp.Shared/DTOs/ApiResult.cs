namespace TodoApp.Shared.DTOs;

/// <summary>
/// API操作の結果を表すDTO
/// </summary>
/// <typeparam name="T">データの型</typeparam>
public class ApiResult<T>
{
    /// <summary>
    /// 操作が成功したかどうか
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// 結果データ
    /// </summary>
    public T? Data { get; set; }
    
    /// <summary>
    /// エラー情報
    /// </summary>
    public ErrorDto? Error { get; set; }
    
    /// <summary>
    /// 成功結果を作成
    /// </summary>
    /// <param name="data">結果データ</param>
    /// <returns>成功結果</returns>
    public static ApiResult<T> Success(T data)
    {
        return new ApiResult<T>
        {
            IsSuccess = true,
            Data = data
        };
    }
    
    /// <summary>
    /// 失敗結果を作成
    /// </summary>
    /// <param name="error">エラー情報</param>
    /// <returns>失敗結果</returns>
    public static ApiResult<T> Failure(ErrorDto error)
    {
        return new ApiResult<T>
        {
            IsSuccess = false,
            Error = error
        };
    }
}