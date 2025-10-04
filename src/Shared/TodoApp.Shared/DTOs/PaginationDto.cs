namespace TodoApp.Shared.DTOs;

/// <summary>
/// ページング情報を表すDTO
/// </summary>
public class PaginationDto
{
    /// <summary>
    /// 現在のページ番号
    /// </summary>
    public int CurrentPage { get; set; }
    
    /// <summary>
    /// 1ページの件数
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// 総件数
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// 総ページ数
    /// </summary>
    public int TotalPages { get; set; }
    
    /// <summary>
    /// 次のページが存在するかどうか
    /// </summary>
    public bool HasNext { get; set; }
    
    /// <summary>
    /// 前のページが存在するかどうか
    /// </summary>
    public bool HasPrevious { get; set; }
}