using System.Text;
using System.Text.Json;
using TodoApp.Shared.DTOs;
using TodoApp.Shared.DTOs.Task;

namespace TodoApp.Frontend.Services;

/// <summary>
/// Task APIとの通信を行うサービス
/// </summary>
public class TaskApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public TaskApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    /// <summary>
    /// タスク一覧を取得する
    /// </summary>
    /// <param name="page">ページ番号</param>
    /// <param name="pageSize">1ページあたりの件数</param>
    /// <param name="search">検索文字列</param>
    /// <param name="status">ステータスフィルター</param>
    /// <param name="sortBy">ソート項目</param>
    /// <param name="sortOrder">ソート順</param>
    /// <returns>ページング済みタスク一覧</returns>
    public async Task<PagedResultDto<TaskDto>?> GetTasksAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null, 
        string? status = null,
        string? sortBy = null,
        string? sortOrder = null)
    {
        var queryParams = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrEmpty(search))
            queryParams.Add($"search={Uri.EscapeDataString(search)}");
        
        if (!string.IsNullOrEmpty(status))
            queryParams.Add($"status={status}");
            
        if (!string.IsNullOrEmpty(sortBy))
            queryParams.Add($"sortBy={sortBy}");
            
        if (!string.IsNullOrEmpty(sortOrder))
            queryParams.Add($"sortOrder={sortOrder}");

        var query = string.Join("&", queryParams);
        var response = await _httpClient.GetAsync($"api/v1/tasks?{query}");

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PagedResultDto<TaskDto>>(json, _jsonOptions);
    }

    /// <summary>
    /// タスクの詳細を取得する
    /// </summary>
    /// <param name="id">タスクID</param>
    /// <returns>タスク詳細</returns>
    public async Task<TaskDto?> GetTaskAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"api/v1/tasks/{id}");
        
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TaskDto>(json, _jsonOptions);
    }

    /// <summary>
    /// タスクを作成する
    /// </summary>
    /// <param name="request">タスク作成リクエスト</param>
    /// <returns>作成されたタスク</returns>
    public async Task<TaskDto?> CreateTaskAsync(CreateTaskRequestDto request)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("api/v1/tasks", content);
        
        if (!response.IsSuccessStatusCode)
            return null;

        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TaskDto>(responseJson, _jsonOptions);
    }

    /// <summary>
    /// タスクを更新する
    /// </summary>
    /// <param name="id">タスクID</param>
    /// <param name="request">タスク更新リクエスト</param>
    /// <returns>更新されたタスク</returns>
    public async Task<TaskDto?> UpdateTaskAsync(Guid id, UpdateTaskRequestDto request)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PutAsync($"api/v1/tasks/{id}", content);
        
        if (!response.IsSuccessStatusCode)
            return null;

        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TaskDto>(responseJson, _jsonOptions);
    }

    /// <summary>
    /// タスクを削除する
    /// </summary>
    /// <param name="id">タスクID</param>
    /// <returns>削除成功可否</returns>
    public async Task<bool> DeleteTaskAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/v1/tasks/{id}");
        return response.IsSuccessStatusCode;
    }
}