# データベース設計書

## 概要

ToDoアプリケーション マイクロサービスのデータベース設計書です。
マイクロサービスアーキテクチャの原則に従い、各サービスが独立したデータベースを持つ設計とし、
データの整合性とパフォーマンスを両立します。

## データベース構成

### 全体構成
```
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│   Task Service  │ │  Label Service  │ │  User Service   │
│   Database      │ │   Database      │ │   Database      │
│   (Azure SQL)   │ │   (Azure SQL)   │ │   (Azure SQL)   │
└─────────────────┘ └─────────────────┘ └─────────────────┘
          │                   │                   │
          └───────────────────┼───────────────────┘
                              │
                    ┌─────────▼─────────┐
                    │    Shared Data    │
                    │  (Event Store)    │
                    │   (Azure SQL)     │
                    └───────────────────┘
```

### データベース一覧
| データベース名 | サービス | 用途 | 接続文字列設定名 |
|---------------|---------|------|------------------|
| TodoApp_Tasks | Task Service | タスク情報管理 | TaskDbConnection |
| TodoApp_Labels | Label Service | ラベル情報管理 | LabelDbConnection |
| TodoApp_Users | User Service | ユーザー情報管理 | UserDbConnection |
| TodoApp_Events | Event Store | イベントソーシング | EventDbConnection |

## Task Service Database

### 概要
タスクの作成、更新、削除、検索に関するデータを管理します。

### テーブル設計

#### 1. Tasks テーブル
**用途**: メインのタスク情報を格納

```sql
CREATE TABLE Tasks (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(2000) NULL,
    Status TINYINT NOT NULL DEFAULT 0, -- 0: Pending, 1: Completed, 2: Deleted
    Priority TINYINT NOT NULL DEFAULT 1, -- 1: Low, 2: Medium, 3: High
    DueDate DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CompletedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    UserId NVARCHAR(450) NOT NULL, -- Azure AD User ID
    VERSION ROWVERSION, -- 楽観的同時実行制御
    
    -- 制約
    CONSTRAINT CK_Tasks_Status CHECK (Status IN (0, 1, 2)),
    CONSTRAINT CK_Tasks_Priority CHECK (Priority IN (1, 2, 3)),
    CONSTRAINT CK_Tasks_DueDate CHECK (DueDate IS NULL OR DueDate > CreatedAt),
    CONSTRAINT CK_Tasks_CompletedAt CHECK (
        (Status = 1 AND CompletedAt IS NOT NULL) OR 
        (Status != 1 AND CompletedAt IS NULL)
    )
);

-- インデックス
CREATE NONCLUSTERED INDEX IX_Tasks_UserId_Status 
    ON Tasks (UserId, Status) 
    INCLUDE (Title, DueDate, Priority, CreatedAt);

CREATE NONCLUSTERED INDEX IX_Tasks_DueDate 
    ON Tasks (DueDate) 
    WHERE IsDeleted = 0 AND Status != 2;

CREATE NONCLUSTERED INDEX IX_Tasks_CreatedAt 
    ON Tasks (CreatedAt DESC);

CREATE NONCLUSTERED INDEX IX_Tasks_UpdatedAt 
    ON Tasks (UpdatedAt DESC);

-- フルテキストインデックス（タイトル・説明の検索用）
CREATE FULLTEXT CATALOG TodoAppFullTextCatalog;

CREATE FULLTEXT INDEX ON Tasks (Title, Description)
    KEY INDEX PK__Tasks__Id
    ON TodoAppFullTextCatalog;
```

**カラム詳細**:
| カラム名 | データ型 | NULL | デフォルト | 説明 |
|---------|---------|------|-----------|------|
| Id | UNIQUEIDENTIFIER | No | NEWID() | 主キー |
| Title | NVARCHAR(200) | No | - | タスクタイトル |
| Description | NVARCHAR(2000) | Yes | NULL | タスク詳細 |
| Status | TINYINT | No | 0 | ステータス（0:未完了, 1:完了, 2:削除済み） |
| Priority | TINYINT | No | 1 | 優先度（1:低, 2:中, 3:高） |
| DueDate | DATETIME2 | Yes | NULL | 期日 |
| CreatedAt | DATETIME2 | No | GETUTCDATE() | 作成日時 |
| UpdatedAt | DATETIME2 | No | GETUTCDATE() | 更新日時 |
| CompletedAt | DATETIME2 | Yes | NULL | 完了日時 |
| IsDeleted | BIT | No | 0 | 削除フラグ |
| DeletedAt | DATETIME2 | Yes | NULL | 削除日時 |
| UserId | NVARCHAR(450) | No | - | ユーザーID（Azure AD） |
| VERSION | ROWVERSION | No | - | バージョン（楽観的ロック） |

#### 2. TaskLabels テーブル
**用途**: タスクとラベルの多対多関係を管理

```sql
CREATE TABLE TaskLabels (
    TaskId UNIQUEIDENTIFIER NOT NULL,
    LabelId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(450) NOT NULL, -- 作成者ID
    
    PRIMARY KEY (TaskId, LabelId),
    
    FOREIGN KEY (TaskId) REFERENCES Tasks(Id) 
        ON DELETE CASCADE
);

-- インデックス
CREATE NONCLUSTERED INDEX IX_TaskLabels_LabelId 
    ON TaskLabels (LabelId);

CREATE NONCLUSTERED INDEX IX_TaskLabels_CreatedAt 
    ON TaskLabels (CreatedAt);
```

#### 3. TaskFiles テーブル
**用途**: タスクに添付されたファイル情報を管理

```sql
CREATE TABLE TaskFiles (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TaskId UNIQUEIDENTIFIER NOT NULL,
    OriginalFileName NVARCHAR(255) NOT NULL,
    StoredFileName NVARCHAR(255) NOT NULL, -- Azure Blob Storage上のファイル名
    FileSize BIGINT NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    BlobContainerName NVARCHAR(100) NOT NULL DEFAULT 'task-files',
    BlobPath NVARCHAR(1000) NOT NULL, -- Blob Storage内のパス
    UploadedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UploadedBy NVARCHAR(450) NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    Checksum NVARCHAR(64) NULL, -- ファイル整合性確認用
    
    FOREIGN KEY (TaskId) REFERENCES Tasks(Id) 
        ON DELETE CASCADE,
        
    CONSTRAINT CK_TaskFiles_FileSize CHECK (FileSize > 0 AND FileSize <= 10485760), -- 10MB制限
    CONSTRAINT CK_TaskFiles_ContentType CHECK (
        ContentType IN (
            'image/jpeg', 'image/png', 'image/gif',
            'application/pdf',
            'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
            'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            'text/plain'
        )
    )
);

-- インデックス
CREATE NONCLUSTERED INDEX IX_TaskFiles_TaskId 
    ON TaskFiles (TaskId) 
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_TaskFiles_UploadedAt 
    ON TaskFiles (UploadedAt DESC);
```

#### 4. TaskAuditLog テーブル
**用途**: タスクの変更履歴を記録

```sql
CREATE TABLE TaskAuditLog (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    TaskId UNIQUEIDENTIFIER NOT NULL,
    Action NVARCHAR(50) NOT NULL, -- CREATE, UPDATE, DELETE, RESTORE
    ChangedFields NVARCHAR(MAX) NULL, -- JSON形式で変更されたフィールド
    OldValues NVARCHAR(MAX) NULL, -- JSON形式で変更前の値
    NewValues NVARCHAR(MAX) NULL, -- JSON形式で変更後の値
    UserId NVARCHAR(450) NOT NULL,
    UserAgent NVARCHAR(500) NULL,
    IpAddress NVARCHAR(45) NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT CK_TaskAuditLog_Action CHECK (
        Action IN ('CREATE', 'UPDATE', 'DELETE', 'RESTORE', 'COMPLETE', 'INCOMPLETE')
    )
);

-- インデックス
CREATE NONCLUSTERED INDEX IX_TaskAuditLog_TaskId_Timestamp 
    ON TaskAuditLog (TaskId, Timestamp DESC);

CREATE NONCLUSTERED INDEX IX_TaskAuditLog_UserId_Timestamp 
    ON TaskAuditLog (UserId, Timestamp DESC);

CREATE NONCLUSTERED INDEX IX_TaskAuditLog_Timestamp 
    ON TaskAuditLog (Timestamp DESC);
```

### トリガー設定

#### タスク更新時の自動処理

```sql
-- タスク更新時にUpdatedAtを自動更新し、監査ログを記録
CREATE TRIGGER TR_Tasks_Update
ON Tasks
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- UpdatedAtの自動更新
    UPDATE t
    SET UpdatedAt = GETUTCDATE()
    FROM Tasks t
    INNER JOIN inserted i ON t.Id = i.Id;
    
    -- 完了ステータス変更時のCompletedAt更新
    UPDATE t
    SET CompletedAt = CASE 
        WHEN i.Status = 1 AND d.Status != 1 THEN GETUTCDATE()
        WHEN i.Status != 1 AND d.Status = 1 THEN NULL
        ELSE t.CompletedAt
    END
    FROM Tasks t
    INNER JOIN inserted i ON t.Id = i.Id
    INNER JOIN deleted d ON t.Id = d.Id
    WHERE i.Status != d.Status;
END;
```

### ストアドプロシージャ

#### タスク検索プロシージャ

```sql
CREATE PROCEDURE sp_SearchTasks
    @UserId NVARCHAR(450),
    @SearchText NVARCHAR(200) = NULL,
    @Status TINYINT = NULL,
    @LabelIds NVARCHAR(MAX) = NULL, -- JSON配列
    @SortBy NVARCHAR(50) = 'CreatedAt',
    @SortOrder NVARCHAR(4) = 'DESC',
    @Page INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    DECLARE @TotalCount INT;
    
    -- ベースクエリ
    WITH TaskCTE AS (
        SELECT DISTINCT t.*
        FROM Tasks t
        LEFT JOIN TaskLabels tl ON t.Id = tl.TaskId
        WHERE t.UserId = @UserId
            AND t.IsDeleted = 0
            AND (@Status IS NULL OR t.Status = @Status)
            AND (@SearchText IS NULL OR 
                 CONTAINS((t.Title, t.Description), @SearchText))
            AND (@LabelIds IS NULL OR 
                 tl.LabelId IN (SELECT value FROM OPENJSON(@LabelIds)))
    ),
    SortedTasks AS (
        SELECT *,
            ROW_NUMBER() OVER (
                ORDER BY 
                    CASE WHEN @SortBy = 'Title' AND @SortOrder = 'ASC' THEN Title END ASC,
                    CASE WHEN @SortBy = 'Title' AND @SortOrder = 'DESC' THEN Title END DESC,
                    CASE WHEN @SortBy = 'DueDate' AND @SortOrder = 'ASC' THEN DueDate END ASC,
                    CASE WHEN @SortBy = 'DueDate' AND @SortOrder = 'DESC' THEN DueDate END DESC,
                    CASE WHEN @SortBy = 'CreatedAt' AND @SortOrder = 'ASC' THEN CreatedAt END ASC,
                    CASE WHEN @SortBy = 'CreatedAt' AND @SortOrder = 'DESC' THEN CreatedAt END DESC,
                    CASE WHEN @SortBy = 'UpdatedAt' AND @SortOrder = 'ASC' THEN UpdatedAt END ASC,
                    CASE WHEN @SortBy = 'UpdatedAt' AND @SortOrder = 'DESC' THEN UpdatedAt END DESC
            ) AS RowNum
        FROM TaskCTE
    )
    
    -- 総件数取得
    SELECT @TotalCount = COUNT(*) FROM TaskCTE;
    
    -- ページネーション結果取得
    SELECT *,
           @TotalCount AS TotalCount,
           CEILING(CAST(@TotalCount AS FLOAT) / @PageSize) AS TotalPages
    FROM SortedTasks
    WHERE RowNum BETWEEN @Offset + 1 AND @Offset + @PageSize
    ORDER BY RowNum;
END;
```

## Label Service Database

### 概要
ラベルの管理を行います。各ユーザーが独自のラベルセットを持ちます。

### テーブル設計

#### Labels テーブル

```sql
CREATE TABLE Labels (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(50) NOT NULL,
    Color NVARCHAR(7) NOT NULL DEFAULT '#6366F1', -- HEX色コード
    Description NVARCHAR(200) NULL,
    UserId NVARCHAR(450) NOT NULL,
    SortOrder INT NOT NULL DEFAULT 0, -- 表示順序
    IsDefault BIT NOT NULL DEFAULT 0, -- システムデフォルトラベル
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    
    CONSTRAINT CK_Labels_Color CHECK (Color LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'),
    CONSTRAINT UQ_Labels_UserName UNIQUE (UserId, Name)
);

-- インデックス
CREATE NONCLUSTERED INDEX IX_Labels_UserId_SortOrder 
    ON Labels (UserId, SortOrder) 
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Labels_UserId_Name 
    ON Labels (UserId, Name) 
    WHERE IsDeleted = 0;
```

#### LabelStats テーブル（統計情報）

```sql
CREATE TABLE LabelStats (
    LabelId UNIQUEIDENTIFIER PRIMARY KEY,
    TotalTasks INT NOT NULL DEFAULT 0,
    CompletedTasks INT NOT NULL DEFAULT 0,
    PendingTasks INT NOT NULL DEFAULT 0,
    LastUsedAt DATETIME2 NULL,
    LastCalculatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (LabelId) REFERENCES Labels(Id) 
        ON DELETE CASCADE,
        
    CONSTRAINT CK_LabelStats_Counts CHECK (
        TotalTasks = CompletedTasks + PendingTasks AND
        TotalTasks >= 0 AND CompletedTasks >= 0 AND PendingTasks >= 0
    )
);
```

## User Service Database

### 概要
ユーザー情報とプロファイル設定を管理します。

### テーブル設計

#### Users テーブル

```sql
CREATE TABLE Users (
    Id NVARCHAR(450) PRIMARY KEY, -- Azure AD Object ID
    Email NVARCHAR(256) NOT NULL,
    DisplayName NVARCHAR(100) NOT NULL,
    FirstName NVARCHAR(50) NULL,
    LastName NVARCHAR(50) NULL,
    ProfileImageUrl NVARCHAR(500) NULL,
    TimeZone NVARCHAR(50) NOT NULL DEFAULT 'UTC',
    Culture NVARCHAR(10) NOT NULL DEFAULT 'en-US',
    IsActive BIT NOT NULL DEFAULT 1,
    LastLoginAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);

-- インデックス
CREATE NONCLUSTERED INDEX IX_Users_Email 
    ON Users (Email);

CREATE NONCLUSTERED INDEX IX_Users_LastLoginAt 
    ON Users (LastLoginAt DESC) 
    WHERE IsActive = 1;
```

#### UserSettings テーブル

```sql
CREATE TABLE UserSettings (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId NVARCHAR(450) NOT NULL,
    SettingKey NVARCHAR(100) NOT NULL,
    SettingValue NVARCHAR(MAX) NOT NULL,
    SettingType NVARCHAR(20) NOT NULL DEFAULT 'string', -- string, int, bool, json
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (UserId) REFERENCES Users(Id) 
        ON DELETE CASCADE,
        
    CONSTRAINT UQ_UserSettings_UserKey UNIQUE (UserId, SettingKey),
    CONSTRAINT CK_UserSettings_Type CHECK (
        SettingType IN ('string', 'int', 'bool', 'json')
    )
);
```

## Event Store Database

### 概要
イベントソーシングパターンによるイベント履歴を管理します。

### テーブル設計

#### Events テーブル

```sql
CREATE TABLE Events (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    StreamId NVARCHAR(100) NOT NULL, -- 集約ID
    StreamType NVARCHAR(50) NOT NULL, -- Task, Label, User
    EventType NVARCHAR(100) NOT NULL,
    EventVersion INT NOT NULL,
    EventData NVARCHAR(MAX) NOT NULL, -- JSON形式
    MetaData NVARCHAR(MAX) NULL, -- JSON形式
    UserId NVARCHAR(450) NOT NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT UQ_Events_Stream_Version UNIQUE (StreamId, EventVersion)
);

-- インデックス
CREATE NONCLUSTERED INDEX IX_Events_StreamId_Version 
    ON Events (StreamId, EventVersion);

CREATE NONCLUSTERED INDEX IX_Events_StreamType_Timestamp 
    ON Events (StreamType, Timestamp DESC);

CREATE NONCLUSTERED INDEX IX_Events_UserId_Timestamp 
    ON Events (UserId, Timestamp DESC);
```

#### Snapshots テーブル

```sql
CREATE TABLE Snapshots (
    StreamId NVARCHAR(100) PRIMARY KEY,
    StreamType NVARCHAR(50) NOT NULL,
    Version INT NOT NULL,
    Data NVARCHAR(MAX) NOT NULL, -- JSON形式
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT UQ_Snapshots_StreamId_Version UNIQUE (StreamId, Version)
);
```

## データベース運用設定

### バックアップ戦略

#### 自動バックアップ設定
```sql
-- Azure SQL Database の自動バックアップ設定
-- ポイントインタイム復旧: 7日間
-- 長期保持バックアップ: 週次4週間、月次12ヶ月、年次10年

-- 手動バックアップ（必要に応じて）
BACKUP DATABASE TodoApp_Tasks 
TO URL = 'https://todoappbackup.blob.core.windows.net/backups/TodoApp_Tasks.bak'
WITH CREDENTIAL = 'TodoAppBackupCredential';
```

### パフォーマンス監視

#### インデックスメンテナンス
```sql
-- インデックス断片化チェック
SELECT 
    DB_NAME() AS DatabaseName,
    OBJECT_NAME(ips.object_id) AS TableName,
    i.name AS IndexName,
    ips.avg_fragmentation_in_percent,
    ips.page_count
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
INNER JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
WHERE ips.avg_fragmentation_in_percent > 10
    AND ips.page_count > 1000
ORDER BY ips.avg_fragmentation_in_percent DESC;

-- インデックス再構築（断片化30%以上）
ALTER INDEX ALL ON Tasks REBUILD;

-- インデックス再編成（断片化10-30%）
ALTER INDEX ALL ON Tasks REORGANIZE;
```

#### 統計情報更新
```sql
-- 統計情報の更新
UPDATE STATISTICS Tasks WITH FULLSCAN;
UPDATE STATISTICS TaskLabels WITH FULLSCAN;
UPDATE STATISTICS TaskFiles WITH FULLSCAN;
```

### セキュリティ設定

#### データマスキング
```sql
-- 動的データマスキング設定
ALTER TABLE Users 
ALTER COLUMN Email ADD MASKED WITH (FUNCTION = 'email()');

ALTER TABLE TaskAuditLog 
ALTER COLUMN IpAddress ADD MASKED WITH (FUNCTION = 'default()');
```

#### 行レベルセキュリティ
```sql
-- ユーザー毎のデータアクセス制御
CREATE SCHEMA Security;

CREATE FUNCTION Security.UserAccessPredicate(@UserId NVARCHAR(450))
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN SELECT 1 AS accessResult 
WHERE @UserId = USER_NAME() OR IS_MEMBER('db_owner') = 1;

CREATE SECURITY POLICY Security.UserAccessPolicy
ADD FILTER PREDICATE Security.UserAccessPredicate(UserId) ON dbo.Tasks,
ADD FILTER PREDICATE Security.UserAccessPredicate(UserId) ON dbo.Labels;
```

### データ保持ポリシー

#### 自動データクリーンアップ
```sql
-- 削除済みタスクの物理削除（90日後）
CREATE PROCEDURE sp_CleanupDeletedTasks
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CleanupDate DATETIME2 = DATEADD(DAY, -90, GETUTCDATE());
    
    -- 関連ファイルの削除
    DELETE FROM TaskFiles 
    WHERE TaskId IN (
        SELECT Id FROM Tasks 
        WHERE IsDeleted = 1 AND DeletedAt < @CleanupDate
    );
    
    -- タスクの物理削除
    DELETE FROM Tasks 
    WHERE IsDeleted = 1 AND DeletedAt < @CleanupDate;
    
    -- 監査ログのアーカイブ（1年後）
    DECLARE @ArchiveDate DATETIME2 = DATEADD(YEAR, -1, GETUTCDATE());
    
    -- アーカイブテーブルに移動
    INSERT INTO TaskAuditLogArchive 
    SELECT * FROM TaskAuditLog 
    WHERE Timestamp < @ArchiveDate;
    
    DELETE FROM TaskAuditLog 
    WHERE Timestamp < @ArchiveDate;
END;
```

---

**作成日**: 2024年1月
**バージョン**: 1.0
**作成者**: データベース設計チーム