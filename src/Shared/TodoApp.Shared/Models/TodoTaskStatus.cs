namespace TodoApp.Shared.Models;

/// <summary>
/// タスクのステータスを表す列挙型
/// </summary>
public enum TodoTaskStatus
{
    /// <summary>
    /// 未完了
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// 完了
    /// </summary>
    Completed = 1,
    
    /// <summary>
    /// 削除済み
    /// </summary>
    Deleted = 2
}