# ToDoアプリケーション 設計書

## 1. 概要

### 1.1 プロジェクト概要
本プロジェクトは、ASP.NET Coreを用いたマイクロサービスアーキテクチャによるToDoアプリケーションのデモンストレーションです。
Azure上でのクラウドネイティブなアプリケーションとして設計され、スケーラビリティ、保守性、セキュリティを重視した実装を行います。

### 1.2 技術スタック
- **フレームワーク**: ASP.NET Core 9.0
- **データベース**: SQL Server 2025
- **クラウドプラットフォーム**: Microsoft Azure
  - Azure Container Apps
  - Azure SQL Database
  - Azure Blob Storage
- **認証**: Microsoft Entra ID (旧Azure AD)
- **API**: RESTful API + OpenAPI (Swagger)
- **監視**: Application Insights

### 1.3 アーキテクチャパターン
- **マイクロサービスアーキテクチャ**
- **ドメイン駆動設計 (DDD)**
- **Command Query Responsibility Segregation (CQRS)** パターン
- **Clean Architecture** パターン

### 1.4 Azure Container Apps を選択した理由
Azure Container Apps は、マイクロサービス デプロイメントに最適化されたサーバーレス コンテナ プラットフォームです：

#### 主要な利点：
- **マイクロサービス最適化**: 複数のマイクロサービスにまたがるアプリケーションに最適化
- **Kubernetes ベース**: Kubernetes、Dapr、KEDA、Envoy などのオープンソース技術を活用
- **サービス ディスカバリ**: Kubernetes スタイルのアプリとマイクロサービスをサポート
- **トラフィック分割**: リビジョン管理とトラフィック分割機能
- **イベント駆動**: キューなどのイベントソースからのスケーリングをサポート
- **ゼロスケール**: 使用量に応じた完全なゼロスケールを含む自動スケーリング
- **コスト効率**: 使用した分だけの課金モデル

#### Azure App Service との比較：
- **コンテナ ネイティブ**: コンテナ化されたワークロードに特化
- **マイクロサービス サポート**: 複数サービス間の通信とサービス ディスカバリ
- **スケーラビリティ**: より効率的なリソース使用とゼロスケール機能
- **DevOps 統合**: コンテナ レジストリとの緊密な統合

## 2. 機能要件

### 2.1 タスク管理機能

#### 2.1.1 タスクの一覧 (List)
- **説明**: システムに登録されているタスクの一覧を表示
- **仕様**:
  - 10件ずつのページング機能
  - ステータス（未完了/完了/削除済み）によるフィルター
  - タイトルの部分一致検索
  - 作成日、更新日、期日による並び替え
- **エンドポイント**: `GET /api/v1/tasks`
- **パラメータ**:
  - `page`: ページ番号 (デフォルト: 1)
  - `pageSize`: 1ページの件数 (デフォルト: 10, 最大: 100)
  - `status`: ステータスフィルター (pending, completed, deleted)
  - `search`: タイトル検索文字列
  - `sortBy`: ソート項目 (createdAt, updatedAt, dueDate, title)
  - `sortOrder`: ソート順 (asc, desc)

#### 2.1.2 タスクの作成 (Create)
- **説明**: 新しいタスクを作成
- **必須入力項目**:
  - タイトル (最大200文字)
  - 詳細内容 (最大2000文字)
  - 期日
- **任意入力項目**:
  - ラベル（複数設定可能、最大10個）
  - 添付ファイル（Azure Blob Storageに保存）
- **初期設定**:
  - ステータス: 「未完了」
  - 作成日時: システム現在時刻
- **エンドポイント**: `POST /api/v1/tasks`

#### 2.1.3 タスクの更新 (Update)
- **説明**: 既存タスクの情報を更新
- **更新可能項目**:
  - タイトル
  - 詳細内容
  - 期日
  - ラベル
  - 添付ファイル
  - ステータス（未完了/完了）
- **ビジネスルール**:
  - ステータスを完了にする場合、完了日時を自動設定
  - 完了から未完了に戻す場合、完了日時をクリア
  - 削除済みタスクは更新不可
- **エンドポイント**: `PUT /api/v1/tasks/{id}`

#### 2.1.4 タスクの削除 (Delete)
- **説明**: タスクを論理削除
- **仕様**:
  - 物理削除ではなく論理削除を実装
  - 削除フラグ (`IsDeleted`) と削除日時 (`DeletedAt`) を設定
  - 削除済みタスクも管理者は閲覧可能
- **エンドポイント**: `DELETE /api/v1/tasks/{id}`

### 2.2 ラベル管理機能
- ラベルの作成、更新、削除
- ラベルによるタスクの分類
- ラベル一覧の取得

### 2.3 ファイル管理機能
- ファイルのアップロード (Azure Blob Storage)
- ファイルのダウンロード
- ファイルの削除
- 対応形式: 画像、PDF、Office文書 (最大10MB)

## 3. 非機能要件

### 3.1 パフォーマンス要件
- **応答時間**: 95%のリクエストが2秒以内
- **スループット**: 1000リクエスト/秒
- **同時接続数**: 500ユーザー

### 3.2 可用性要件
- **稼働率**: 99.5%以上
- **RTO**: 4時間以内
- **RPO**: 1時間以内

### 3.3 スケーラビリティ要件
- **水平スケーリング**: Azure Container Apps の自動スケーリング
- **データベーススケーリング**: Azure SQL Databaseの読み取りレプリカ

### 3.4 セキュリティ要件
- **認証**: Microsoft Entra ID
- **認可**: ロールベースアクセス制御 (RBAC)
- **データ暗号化**: 転送時・保存時の暗号化
- **監査ログ**: 全API操作の記録

## 4. システムアーキテクチャ

### 4.1 マイクロサービス構成

```
┌─────────────────────────────────────────────────────────────┐
│                      API Gateway                           │
│                   (Azure API Management)                   │
└─────────────────────────────────────────────────────────────┘
                                │
                ┌───────────────┼───────────────┐
                │               │               │
        ┌───────▼──────┐ ┌──────▼──────┐ ┌─────▼──────┐
        │   Task API   │ │  Label API  │ │  File API  │
        │(Container App)│ │(Container App)│ │(Container App)│
        └───────┬──────┘ └──────┬──────┘ └─────┬──────┘
                │               │               │
        ┌───────▼──────┐ ┌──────▼──────┐ ┌─────▼──────┐
        │   Task DB    │ │  Label DB   │ │    Blob    │
        │  (Azure SQL) │ │ (Azure SQL) │ │  Storage   │
        └──────────────┘ └─────────────┘ └────────────┘
```

### 4.2 サービス分割
1. **Task Service**: タスクのCRUD操作
2. **Label Service**: ラベルの管理
3. **File Service**: ファイルのアップロード・ダウンロード
4. **User Service**: ユーザー認証・認可
5. **Notification Service**: 通知機能

### 4.3 Container Apps Environment
Azure Container Apps Environment は、関連するコンテナ アプリのグループに対してセキュリティ境界を提供します：

- **共有インフラ**: 同一環境内のアプリは共有インフラストラクチャを使用
- **ネットワーク分離**: VNet 統合によるプライベート ネットワーク
- **Dapr 統合**: マイクロサービス間通信の簡素化
- **ログ統合**: 統一されたログ収集と監視

### 4.4 Dapr 統合
Distributed Application Runtime (Dapr) により、マイクロサービス開発を簡素化：

- **サービス間通信**: mTLS、再試行、タイムアウトを含むサービス呼び出し
- **状態管理**: 分散状態ストアへの一貫したアクセス
- **Pub/Sub メッセージング**: 疎結合なメッセージング パターン
- **可観測性**: 分散トレーシングとメトリクス収集

### 4.5 データ整合性
- **Saga Pattern**: 分散トランザクションの管理
- **Event Sourcing**: 状態変更の履歴管理
- **Eventual Consistency**: 結果整合性の採用

## 5. データベース設計

### 5.1 Task Service Database

#### 5.1.1 Tasks テーブル
```sql
CREATE TABLE Tasks (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(2000),
    Status TINYINT NOT NULL DEFAULT 0, -- 0: Pending, 1: Completed
    DueDate DATETIME2,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CompletedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    UserId NVARCHAR(450) NOT NULL,
    VERSION ROWVERSION
);

CREATE NONCLUSTERED INDEX IX_Tasks_UserId_Status ON Tasks (UserId, Status) INCLUDE (Title, DueDate);
CREATE NONCLUSTERED INDEX IX_Tasks_DueDate ON Tasks (DueDate) WHERE IsDeleted = 0;
```

#### 5.1.2 TaskLabels テーブル
```sql
CREATE TABLE TaskLabels (
    TaskId UNIQUEIDENTIFIER NOT NULL,
    LabelId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    PRIMARY KEY (TaskId, LabelId),
    FOREIGN KEY (TaskId) REFERENCES Tasks(Id) ON DELETE CASCADE
);
```

#### 5.1.3 TaskFiles テーブル
```sql
CREATE TABLE TaskFiles (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TaskId UNIQUEIDENTIFIER NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    FileSize BIGINT NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    BlobUrl NVARCHAR(1000) NOT NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (TaskId) REFERENCES Tasks(Id) ON DELETE CASCADE
);
```

### 5.2 Label Service Database

#### 5.2.1 Labels テーブル
```sql
CREATE TABLE Labels (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(50) NOT NULL,
    Color NVARCHAR(7) NOT NULL DEFAULT '#6366F1', -- HEX color code
    UserId NVARCHAR(450) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    UNIQUE (UserId, Name)
);
```

## 6. API設計

### 6.1 RESTful API 設計原則
- **リソース指向**: URLはリソースを表現
- **HTTPメソッド**: 適切なHTTPメソッドの使用
- **ステータスコード**: 適切なHTTPステータスコードの返却
- **コンテンツネゴシエーション**: Accept/Content-Typeヘッダーの活用

### 6.2 Task API エンドポイント

#### 6.2.1 タスク一覧取得
```http
GET /api/v1/tasks?page=1&pageSize=10&status=pending&search=会議
Authorization: Bearer {JWT_TOKEN}
```

**レスポンス例**:
```json
{
  "data": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "title": "プロジェクト会議の準備",
      "description": "来週の企画会議の資料を準備する",
      "status": "pending",
      "dueDate": "2024-01-15T10:00:00Z",
      "labels": [
        {"id": "label1", "name": "仕事", "color": "#FF6B6B"}
      ],
      "files": [
        {"id": "file1", "fileName": "agenda.pdf", "fileSize": 1024}
      ],
      "createdAt": "2024-01-01T09:00:00Z",
      "updatedAt": "2024-01-01T09:00:00Z"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalCount": 25,
    "totalPages": 3,
    "hasNext": true,
    "hasPrevious": false
  }
}
```

#### 6.2.2 タスク作成
```http
POST /api/v1/tasks
Content-Type: application/json
Authorization: Bearer {JWT_TOKEN}

{
  "title": "新しいタスク",
  "description": "タスクの詳細説明",
  "dueDate": "2024-01-15T10:00:00Z",
  "labelIds": ["label1", "label2"]
}
```

### 6.3 エラーハンドリング

#### 6.3.1 標準エラーレスポンス
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "入力データに問題があります",
    "details": [
      {
        "field": "title",
        "message": "タイトルは必須です"
      }
    ]
  },
  "timestamp": "2024-01-01T09:00:00Z",
  "requestId": "req-123456"
}
```

## 7. セキュリティ設計

### 7.1 認証・認可
- **認証プロバイダー**: Microsoft Entra ID
- **トークン**: JWT (JSON Web Token)
- **認可**: ロールベースアクセス制御 (RBAC)

### 7.2 セキュリティ対策
- **HTTPS**: 全通信の暗号化
- **CORS**: Cross-Origin Resource Sharing設定
- **Rate Limiting**: API呼び出し頻度制限
- **Input Validation**: 入力値検証
- **SQL Injection**: パラメータ化クエリの使用

### 7.3 データ保護
- **個人情報**: GDPR準拠
- **データ暗号化**: Azure Key Vaultを使用
- **監査ログ**: 全操作の記録・監視

## 8. デプロイメント設計

### 8.1 Azure リソース構成
```yaml
# Azure Resource Manager Template (概要)
resources:
  - type: Microsoft.App/managedEnvironments
    name: todoapp-env
    properties:
      appLogsConfiguration:
        destination: log-analytics
  
  - type: Microsoft.App/containerApps
    name: todoapp-task-api
    environmentId: todoapp-env
    properties:
      configuration:
        ingress:
          external: true
          targetPort: 8080
  
  - type: Microsoft.Sql/servers
    name: todoapp-sql-server
  
  - type: Microsoft.Sql/servers/databases
    name: TodoAppDB
    tier: S1
  
  - type: Microsoft.Storage/storageAccounts
    name: todoappblob
    kind: StorageV2
```

### 8.2 CI/CDパイプライン
```yaml
# Azure DevOps Pipeline (概要)
trigger:
  branches:
    include:
      - main
      - develop

stages:
  - stage: Build
    jobs:
      - job: BuildAndTest
        steps:
          - task: DotNetCoreCLI@2
            inputs:
              command: 'restore'
          - task: DotNetCoreCLI@2
            inputs:
              command: 'build'
          - task: DotNetCoreCLI@2
            inputs:
              command: 'test'
          - task: Docker@2
            inputs:
              command: 'buildAndPush'
              repository: 'todoapp/task-api'
              containerRegistry: 'todoappregistry'
  
  - stage: Deploy
    condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
    jobs:
      - deployment: DeployToProduction
        environment: 'production'
        strategy:
          runOnce:
            deploy:
              steps:
                - task: AzureContainerApps@1
                  inputs:
                    azureSubscription: 'Azure-Connection'
                    containerAppName: 'todoapp-task-api'
                    resourceGroup: 'todoapp-rg'
                    imageToDeploy: 'todoappregistry.azurecr.io/todoapp/task-api:$(Build.BuildId)'
```

## 9. 監視・ログ設計

### 9.1 Application Insights設定
- **パフォーマンス監視**: レスポンス時間、スループット
- **例外監視**: エラー発生状況の追跡
- **依存関係監視**: データベース、外部サービスの状況
- **カスタムメトリクス**: ビジネス固有の指標

### 9.2 構造化ログ
```csharp
// 構造化ログの例
logger.LogInformation("Task created successfully. TaskId: {TaskId}, UserId: {UserId}", 
    task.Id, user.Id);

logger.LogWarning("Task update failed. TaskId: {TaskId}, Reason: {Reason}", 
    taskId, "Task not found");
```

### 9.3 ヘルスチェック
```csharp
// ヘルスチェック設定
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString)
    .AddAzureBlobStorage(blobConnectionString)
    .AddCheck<TaskServiceHealthCheck>("task-service");
```

## 10. テスト戦略

### 10.1 テストピラミッド
- **単体テスト**: 70% - ビジネスロジックのテスト
- **統合テスト**: 20% - API エンドポイントのテスト
- **E2Eテスト**: 10% - ユーザーシナリオのテスト

### 10.2 テスト種別
```csharp
// 単体テストの例
[Test]
public async Task CreateTask_ValidInput_ReturnsSuccess()
{
    // Arrange
    var task = new CreateTaskRequest 
    { 
        Title = "Test Task",
        Description = "Test Description",
        DueDate = DateTime.UtcNow.AddDays(1)
    };
    
    // Act
    var result = await _taskService.CreateTaskAsync(task);
    
    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Data.Title, Is.EqualTo("Test Task"));
}

// 統合テストの例
[Test]
public async Task GetTasks_ValidRequest_ReturnsPagedResult()
{
    // Arrange
    await SeedTestData();
    
    // Act
    var response = await _client.GetAsync("/api/v1/tasks?page=1&pageSize=5");
    
    // Assert
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<PagedResult<TaskDto>>();
    Assert.That(result.Data.Count, Is.EqualTo(5));
}
```

## 11. パフォーマンス最適化

### 11.1 キャッシュ戦略
- **Redis Cache**: セッション、一時データ
- **CDN**: 静的コンテンツ
- **Application Cache**: 設定値、マスタデータ

### 11.2 データベース最適化
- **インデックス**: 頻繁に検索される列にインデックス作成
- **クエリ最適化**: N+1問題の解決
- **接続プール**: 効率的なデータベース接続管理

### 11.3 ファイル処理最適化
- **非同期処理**: ファイルアップロードの非同期化
- **ストリーミング**: 大きなファイルのストリーミング処理
- **圧縮**: 画像の自動圧縮

## 12. 運用・保守設計

### 12.1 バックアップ戦略
- **データベース**: 日次自動バックアップ
- **ファイル**: Azure Blob Storage の冗長化
- **設定**: Infrastructure as Code による管理

### 12.2 災害復旧
- **RTO**: 4時間以内
- **RPO**: 1時間以内
- **Multi-Region**: 複数リージョンでの冗長化

### 12.3 セキュリティ更新
- **定期更新**: 月次セキュリティパッチ適用
- **脆弱性スキャン**: 週次自動スキャン
- **ペネトレーションテスト**: 半年毎の実施

## 13. 今後の拡張性

### 13.1 機能拡張
- **通知機能**: メール、プッシュ通知
- **コメント機能**: タスクへのコメント追加
- **チーム機能**: チームでのタスク共有
- **レポート機能**: 生産性分析レポート

### 13.2 技術的拡張
- **マイクロサービス分割**: より細かい粒度での分割
- **イベント駆動**: Event Sourcingの本格導入
- **AI統合**: 自然言語処理による自動分類
- **モバイルアプリ**: ネイティブアプリ対応

## 14. まとめ

本設計書では、ASP.NET Coreを用いたマイクロサービスアーキテクチャによるToDoアプリケーションの包括的な設計を提示しました。クラウドネイティブなアプローチを採用し、スケーラビリティ、セキュリティ、保守性を重視した設計となっています。

Azureの各種サービスを効果的に活用し、現代的な開発・運用プラクティスを実装することで、エンタープライズレベルでの要求に対応できるシステムとして設計されています。

---

**作成日**: 2024年1月
**バージョン**: 1.0
**作成者**: ASP.NET Core マイクロサービス開発チーム