using Microsoft.EntityFrameworkCore;
using TodoApp.Services.Task.Models;
using TodoApp.Shared.Models;

namespace TodoApp.Services.Task.Data;

/// <summary>
/// タスクサービス用DbContext
/// </summary>
public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
    {
    }
    
    /// <summary>
    /// タスク
    /// </summary>
    public DbSet<TaskEntity> Tasks { get; set; }
    
    /// <summary>
    /// タスクラベル
    /// </summary>
    public DbSet<TaskLabelEntity> TaskLabels { get; set; }
    
    /// <summary>
    /// タスクファイル
    /// </summary>
    public DbSet<TaskFileEntity> TaskFiles { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Tasksテーブルの設定
        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.ToTable("Tasks", t =>
            {
                t.HasCheckConstraint("CK_Tasks_Status", "[Status] IN (0, 1, 2)");
                t.HasCheckConstraint("CK_Tasks_Priority", "[Priority] IN (1, 2, 3)");
            });
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Description)
                .HasMaxLength(2000);
            
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>();
            
            entity.Property(e => e.Priority)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(Priority.Medium);
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);
            
            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(450);

            if (Database.IsSqlite())
            {
                // SQLite: BLOB + randomblob で既定値を与える（NOT NULL 回避）
                entity.Property(e => e.Version)
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnType("BLOB")
                    .HasDefaultValueSql("randomblob(8)");
            }
            else if (Database.IsSqlServer())
            {
                // SQL Server: rowversion を使う（DB 側で自動生成、EF の楽観ロック対応）
                entity.Property(e => e.Version)
                    .IsRowVersion(); // これで byte[] が rowversion として扱われる
            }
            else
            {
                // その他プロバイダ向けのフォールバック（必要に応じて調整）
                entity.Property(e => e.Version)
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();
            }

            // インデックス
            entity.HasIndex(e => new { e.UserId, e.Status })
                .HasDatabaseName("IX_Tasks_UserId_Status");
            
            entity.HasIndex(e => e.DueDate)
                .HasDatabaseName("IX_Tasks_DueDate")
                .HasFilter("[IsDeleted] = 0");
            
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_Tasks_CreatedAt");
            
            entity.HasIndex(e => e.UpdatedAt)
                .HasDatabaseName("IX_Tasks_UpdatedAt");
        });
        
        // TaskLabelsテーブルの設定
        modelBuilder.Entity<TaskLabelEntity>(entity =>
        {
            entity.ToTable("TaskLabels");
            
            entity.HasKey(e => new { e.TaskId, e.LabelId });
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(450);
            
            // 外部キー
            entity.HasOne(e => e.Task)
                .WithMany(t => t.TaskLabels)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // インデックス
            entity.HasIndex(e => e.LabelId)
                .HasDatabaseName("IX_TaskLabels_LabelId");
            
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_TaskLabels_CreatedAt");
        });
        
        // TaskFilesテーブルの設定
        modelBuilder.Entity<TaskFileEntity>(entity =>
        {
            entity.ToTable("TaskFiles", t =>
            {
                t.HasCheckConstraint("CK_TaskFiles_FileSize", "[FileSize] > 0 AND [FileSize] <= 10485760"); // 10MB
            });
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");
            
            entity.Property(e => e.OriginalFileName)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.StoredFileName)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.ContentType)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.BlobContainerName)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("task-files");
            
            entity.Property(e => e.BlobPath)
                .IsRequired()
                .HasMaxLength(1000);
            
            entity.Property(e => e.UploadedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            entity.Property(e => e.UploadedBy)
                .IsRequired()
                .HasMaxLength(450);
            
            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);
            
            entity.Property(e => e.Checksum)
                .HasMaxLength(64);
            
            // 外部キー
            entity.HasOne(e => e.Task)
                .WithMany(t => t.TaskFiles)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // インデックス
            entity.HasIndex(e => e.TaskId)
                .HasDatabaseName("IX_TaskFiles_TaskId")
                .HasFilter("[IsDeleted] = 0");
            
            entity.HasIndex(e => e.UploadedAt)
                .HasDatabaseName("IX_TaskFiles_UploadedAt");
        });
    }
    
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }
    
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<TaskEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
        
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            
            entry.Entity.UpdatedAt = DateTime.UtcNow;
            
            // ステータスが完了に変更された場合、完了日時を設定
            if (entry.State == EntityState.Modified)
            {
                var originalStatus = entry.OriginalValues.GetValue<TodoTaskStatus>(nameof(TaskEntity.Status));
                var currentStatus = entry.Entity.Status;
                
                if (originalStatus != TodoTaskStatus.Completed && currentStatus == TodoTaskStatus.Completed)
                {
                    entry.Entity.CompletedAt = DateTime.UtcNow;
                }
                else if (originalStatus == TodoTaskStatus.Completed && currentStatus != TodoTaskStatus.Completed)
                {
                    entry.Entity.CompletedAt = null;
                }
            }
        }
    }
}