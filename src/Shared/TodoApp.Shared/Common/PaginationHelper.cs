namespace TodoApp.Shared.Common;

/// <summary>
/// ページングヘルパー
/// </summary>
public static class PaginationHelper
{
    /// <summary>
    /// ページング情報を計算
    /// </summary>
    /// <param name="totalCount">総件数</param>
    /// <param name="page">現在のページ番号</param>
    /// <param name="pageSize">1ページの件数</param>
    /// <returns>ページング情報</returns>
    public static (int skip, int take, int totalPages, bool hasNext, bool hasPrevious) Calculate(
        int totalCount, 
        int page, 
        int pageSize)
    {
        // ページ番号とページサイズのバリデーション
        page = Math.Max(1, page);
        pageSize = Math.Min(Math.Max(1, pageSize), 100); // 最大100件
        
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var skip = (page - 1) * pageSize;
        var hasNext = page < totalPages;
        var hasPrevious = page > 1;
        
        return (skip, pageSize, totalPages, hasNext, hasPrevious);
    }
}