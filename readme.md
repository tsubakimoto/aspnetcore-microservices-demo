# ToDoアプリケーション - ASP.NET Core マイクロサービスデモ

ASP.NET Core 9.0を使用したマイクロサービスアーキテクチャによるToDoアプリケーションのデモンストレーションです。

## 🏗️ アーキテクチャ概要

このプロジェクトはマイクロサービスアーキテクチャの原則に基づいて設計されており、以下のサービスで構成されています：

- **Task Service** (完全実装済み) - タスクのCRUD操作、ページング、フィルタリング、ソート機能
- **Label Service** - ラベル管理機能
- **File Service** - ファイルアップロード・ダウンロード機能
- **Shared Library** - 共通DTOとモデル

## 📋 前提条件

### 必要な環境

- **.NET 9.0 SDK** またはそれ以降
- **Git**
- **Visual Studio Code** または **Visual Studio 2022** (推奨)

### オプション環境

- **Docker Desktop** (コンテナ実行用、将来実装予定)
- **SQL Server** (本番環境用、開発環境ではSQLiteを自動使用)

## 🚀 クイックスタート

### 1. リポジトリのクローン

```bash
git clone https://github.com/tsubakimoto/aspnetcore-microservices-demo.git
cd aspnetcore-microservices-demo
```

### 2. 依存関係の復元

```bash
# ソリューション全体の依存関係を復元
dotnet restore
```

### 3. アプリケーションのビルド

```bash
# 全プロジェクトをビルド
dotnet build
```

### 4. Task Serviceの実行

```bash
# Task Serviceディレクトリに移動
cd src/Services/Task/TodoApp.Services.Task

# アプリケーションを実行
dotnet run
```

または、ソリューションルートから：

```bash
dotnet run --project src/Services/Task/TodoApp.Services.Task/TodoApp.Services.Task.csproj
```

### 5. API動作確認

Task Serviceが起動したら、ブラウザで以下のURLにアクセス：

- **Swagger UI**: `http://localhost:5105` または `https://localhost:7077`
- **ヘルスチェック**: `http://localhost:5105/health`

## 🧪 テストの実行

### 全テストの実行

```bash
# ソリューション全体のテストを実行
dotnet test
```

### 特定プロジェクトのテスト実行

```bash
# Task Serviceのテストのみ実行
dotnet test tests/Task.Tests/TodoApp.Services.Task.Tests/TodoApp.Services.Task.Tests.csproj
```

## 📁 プロジェクト構造

```
aspnetcore-microservices-demo/
├── src/                              # ソースコードディレクトリ
│   ├── Services/                     # マイクロサービス
│   │   ├── Task/                     # タスクサービス
│   │   │   └── TodoApp.Services.Task/
│   │   ├── Label/                    # ラベルサービス
│   │   │   └── TodoApp.Services.Label/
│   │   └── File/                     # ファイルサービス
│   │       └── TodoApp.Services.File/
│   └── Shared/                       # 共有ライブラリ
│       └── TodoApp.Shared/
├── tests/                            # テストプロジェクト
│   ├── Task.Tests/
│   ├── Label.Tests/
│   └── File.Tests/
├── design-docs/                      # 設計書
├── infra/                           # インフラストラクチャコード (将来実装)
└── TodoApp.sln                     # ソリューションファイル
```

## 🔧 開発環境セットアップ

### Visual Studio Code

推奨拡張機能：

```bash
# C# Dev Kit
code --install-extension ms-dotnettools.csdevkit

# C# Extensions
code --install-extension ms-dotnettools.csharp

# NuGet Package Manager
code --install-extension jmrog.vscode-nuget-package-manager
```

### データベース

- **開発環境**: SQLiteが自動的に作成・使用されます
- **本番環境**: SQL Serverを使用（接続文字列設定が必要）

## 🌐 API エンドポイント

### Task Service API (`http://localhost:5105`)

| Method | Endpoint | 説明 |
|--------|----------|------|
| `GET` | `/api/v1/tasks` | タスク一覧取得（ページング対応） |
| `GET` | `/api/v1/tasks/{id}` | タスク詳細取得 |
| `POST` | `/api/v1/tasks` | タスク作成 |
| `PUT` | `/api/v1/tasks/{id}` | タスク更新 |
| `DELETE` | `/api/v1/tasks/{id}` | タスク削除 |

### クエリパラメータ

- `page`: ページ番号 (デフォルト: 1)
- `pageSize`: 1ページの件数 (デフォルト: 10, 最大: 100)
- `status`: ステータスフィルター (`Pending`, `Completed`, `Deleted`)
- `search`: 検索文字列（タイトル・詳細内容を対象）
- `sortBy`: ソート項目 (`title`, `dueDate`, `createdAt`, `updatedAt`, `priority`)
- `sortOrder`: ソート順 (`asc`, `desc`)
- `labelIds`: ラベルIDフィルター（カンマ区切り）

### 使用例

```bash
# 基本的なタスク一覧取得
curl "http://localhost:5105/api/v1/tasks"

# ページング付きでタスク取得
curl "http://localhost:5105/api/v1/tasks?page=1&pageSize=5"

# ステータスフィルター付き
curl "http://localhost:5105/api/v1/tasks?status=Pending"

# 検索機能
curl "http://localhost:5105/api/v1/tasks?search=重要"

# タスク作成
curl -X POST "http://localhost:5105/api/v1/tasks" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "新しいタスク",
    "description": "タスクの詳細",
    "priority": 2,
    "dueDate": "2024-12-31T23:59:59Z"
  }'
```

## 🐛 トラブルシューティング

### 一般的な問題

1. **ポートが既に使用されている**
   ```bash
   # 別のポートを指定して実行
   dotnet run --urls "http://localhost:5106"
   ```

2. **データベース接続エラー**
   - 開発環境ではSQLiteが自動作成されるため、通常は発生しません
   - SQLiteファイルの権限を確認してください

3. **ビルドエラー**
   ```bash
   # NuGetキャッシュをクリア
   dotnet nuget locals all --clear
   
   # 依存関係を再復元
   dotnet restore --force
   ```

### ログの確認

```bash
# 詳細なログ出力でアプリケーションを実行
dotnet run --verbosity detailed
```

## 📚 参考資料

- [設計書](./design-docs/README.md)
- [API仕様書](./design-docs/api-specification.md)
- [データベース設計書](./design-docs/database-design.md)
- [Azure インフラストラクチャ設計書](./design-docs/azure-infrastructure.md)

## 🎯 実装状況

- ✅ **Task Service**: 完全実装済み（CRUD、ページング、フィルタリング、ソート）
- ⏳ **Label Service**: 基本構造のみ実装
- ⏳ **File Service**: 基本構造のみ実装
- ✅ **共有ライブラリ**: 完全実装済み
- ✅ **テストフレームワーク**: 設定済み

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.