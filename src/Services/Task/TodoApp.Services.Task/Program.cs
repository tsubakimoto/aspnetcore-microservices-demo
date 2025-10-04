using Microsoft.EntityFrameworkCore;
using TodoApp.Services.Task.Data;
using TodoApp.Services.Task.Services;

var builder = WebApplication.CreateBuilder(args);

// サービスの登録
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Task Service API", 
        Version = "v1",
        Description = "ToDoアプリケーション - タスクサービスAPI"
    });
    
    // XMLコメントを有効化
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Entity Framework Core設定
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    // 開発環境ではSQLiteを使用
    connectionString = "Data Source=TaskService.db";
    builder.Services.AddDbContext<TaskDbContext>(options =>
        options.UseSqlite(connectionString));
}
else
{
    // 本番環境ではSQL Serverを使用
    builder.Services.AddDbContext<TaskDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// サービスの依存性注入
builder.Services.AddScoped<ITaskService, TaskService>();

// CORS設定
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ヘルスチェック
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString, name: "database");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Service API v1");
        c.RoutePrefix = string.Empty; // Swaggerをルートで表示
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// ヘルスチェックエンドポイント
app.MapHealthChecks("/health");

// データベース初期化
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    
    // 開発環境では自動でマイグレーション実行
    if (app.Environment.IsDevelopment())
    {
        await context.Database.EnsureCreatedAsync();
    }
}

app.Run();
