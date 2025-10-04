using Microsoft.AspNetCore.Mvc;
using TodoApp.Services.Task.Services;
using TodoApp.Shared.DTOs;
using TodoApp.Shared.DTOs.Task;
using TodoApp.Shared.Constants;

namespace TodoApp.Services.Task.Controllers;

/// <summary>
/// タスク管理API
/// </summary>
[ApiController]
[Route("api/v1/tasks")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    /// <summary>
    /// タスク一覧取得
    /// </summary>
    /// <param name="page">ページ番号</param>
    /// <param name="pageSize">ページサイズ</param>
    /// <param name="status">ステータスフィルター</param>
    /// <param name="search">検索文字列</param>
    /// <param name="sortBy">ソート項目</param>
    /// <param name="sortOrder">ソート順</param>
    /// <param name="labelIds">ラベルIDフィルター</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>タスク一覧</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<TaskDto>), 200)]
    [ProducesResponseType(typeof(ErrorDto), 400)]
    [ProducesResponseType(typeof(ErrorDto), 500)]
    public async Task<ActionResult<PagedResultDto<TaskDto>>> GetTasks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortOrder = "desc",
        [FromQuery] string? labelIds = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: 認証からユーザーIDを取得
            var userId = "demo-user"; // 暫定

            // ラベルIDのパース
            var labelIdList = new List<Guid>();
            if (!string.IsNullOrEmpty(labelIds))
            {
                var labelIdStrings = labelIds.Split(',');
                foreach (var labelIdString in labelIdStrings)
                {
                    if (Guid.TryParse(labelIdString.Trim(), out var labelId))
                    {
                        labelIdList.Add(labelId);
                    }
                }
            }

            var result = await _taskService.GetTasksAsync(
                userId, page, pageSize, status, search, sortBy, sortOrder, labelIdList, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "タスク一覧取得でエラーが発生しました");
            return StatusCode(500, new ErrorDto
            {
                Code = ErrorCodes.InternalError,
                Message = "内部サーバーエラーが発生しました",
                RequestId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// タスク詳細取得
    /// </summary>
    /// <param name="id">タスクID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>タスク詳細</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), 200)]
    [ProducesResponseType(typeof(ErrorDto), 404)]
    [ProducesResponseType(typeof(ErrorDto), 500)]
    public async Task<ActionResult<TaskDto>> GetTask(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: 認証からユーザーIDを取得
            var userId = "demo-user"; // 暫定

            var task = await _taskService.GetTaskByIdAsync(id, userId, cancellationToken);
            if (task == null)
            {
                return NotFound(new ErrorDto
                {
                    Code = ErrorCodes.NotFound,
                    Message = "指定されたタスクが見つかりません",
                    RequestId = HttpContext.TraceIdentifier
                });
            }

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "タスク詳細取得でエラーが発生しました。TaskId: {TaskId}", id);
            return StatusCode(500, new ErrorDto
            {
                Code = ErrorCodes.InternalError,
                Message = "内部サーバーエラーが発生しました",
                RequestId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// タスク作成
    /// </summary>
    /// <param name="request">タスク作成リクエスト</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>作成されたタスク</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), 201)]
    [ProducesResponseType(typeof(ErrorDto), 400)]
    [ProducesResponseType(typeof(ErrorDto), 500)]
    public async Task<ActionResult<TaskDto>> CreateTask(
        [FromBody] CreateTaskRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorDto
                {
                    Code = ErrorCodes.ValidationError,
                    Message = "入力データに問題があります",
                    Details = ModelState.SelectMany(x => x.Value?.Errors ?? [])
                        .Select(e => new ErrorDetailDto
                        {
                            Field = "Model",
                            Message = e.ErrorMessage
                        }),
                    RequestId = HttpContext.TraceIdentifier
                });
            }

            // TODO: 認証からユーザーIDを取得
            var userId = "demo-user"; // 暫定

            var task = await _taskService.CreateTaskAsync(request, userId, cancellationToken);

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "タスク作成でエラーが発生しました");
            return StatusCode(500, new ErrorDto
            {
                Code = ErrorCodes.InternalError,
                Message = "内部サーバーエラーが発生しました",
                RequestId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// タスク更新
    /// </summary>
    /// <param name="id">タスクID</param>
    /// <param name="request">タスク更新リクエスト</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>更新されたタスク</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), 200)]
    [ProducesResponseType(typeof(ErrorDto), 400)]
    [ProducesResponseType(typeof(ErrorDto), 404)]
    [ProducesResponseType(typeof(ErrorDto), 500)]
    public async Task<ActionResult<TaskDto>> UpdateTask(
        Guid id, 
        [FromBody] UpdateTaskRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorDto
                {
                    Code = ErrorCodes.ValidationError,
                    Message = "入力データに問題があります",
                    Details = ModelState.SelectMany(x => x.Value?.Errors ?? [])
                        .Select(e => new ErrorDetailDto
                        {
                            Field = "Model",
                            Message = e.ErrorMessage
                        }),
                    RequestId = HttpContext.TraceIdentifier
                });
            }

            // TODO: 認証からユーザーIDを取得
            var userId = "demo-user"; // 暫定

            var task = await _taskService.UpdateTaskAsync(id, request, userId, cancellationToken);
            if (task == null)
            {
                return NotFound(new ErrorDto
                {
                    Code = ErrorCodes.NotFound,
                    Message = "指定されたタスクが見つかりません",
                    RequestId = HttpContext.TraceIdentifier
                });
            }

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "タスク更新でエラーが発生しました。TaskId: {TaskId}", id);
            return StatusCode(500, new ErrorDto
            {
                Code = ErrorCodes.InternalError,
                Message = "内部サーバーエラーが発生しました",
                RequestId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// タスク削除
    /// </summary>
    /// <param name="id">タスクID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>削除結果</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorDto), 404)]
    [ProducesResponseType(typeof(ErrorDto), 500)]
    public async Task<ActionResult> DeleteTask(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: 認証からユーザーIDを取得
            var userId = "demo-user"; // 暫定

            var result = await _taskService.DeleteTaskAsync(id, userId, cancellationToken);
            if (!result)
            {
                return NotFound(new ErrorDto
                {
                    Code = ErrorCodes.NotFound,
                    Message = "指定されたタスクが見つかりません",
                    RequestId = HttpContext.TraceIdentifier
                });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "タスク削除でエラーが発生しました。TaskId: {TaskId}", id);
            return StatusCode(500, new ErrorDto
            {
                Code = ErrorCodes.InternalError,
                Message = "内部サーバーエラーが発生しました",
                RequestId = HttpContext.TraceIdentifier
            });
        }
    }
}
