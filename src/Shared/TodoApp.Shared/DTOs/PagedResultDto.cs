namespace TodoApp.Shared.DTOs;

/// <summary>
/// ページング対応の結果を表すDTO
/// </summary>
/// <typeparam name="T">データの型</typeparam>
public class PagedResultDto<T>
{
    /// <summary>
    /// データのリスト
    /// </summary>
    public IEnumerable<T> Data { get; set; } = new List<T>();
    
    /// <summary>
    /// ページング情報
    /// </summary>
    public PaginationDto Pagination { get; set; } = new PaginationDto();
}