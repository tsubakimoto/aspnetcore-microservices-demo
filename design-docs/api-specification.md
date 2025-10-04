# API 仕様書

## 概要
ToDoアプリケーション マイクロサービスのAPI仕様書です。
RESTful APIの設計原則に従い、OpenAPI 3.0仕様に準拠した実装を行います。

## 基本情報

- **ベースURL**: `https://api.todoapp.example.com`
- **APIバージョン**: v1
- **認証方式**: Bearer Token (JWT)
- **コンテンツタイプ**: `application/json`

## 認証

### JWT Token
すべてのAPIエンドポイントは、有効なJWTトークンによる認証が必要です。

```http
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Task Service API

### タスク一覧取得

**エンドポイント**: `GET /api/v1/tasks`

**説明**: ユーザーのタスク一覧を取得します。

**クエリパラメータ**:
| パラメータ | 型 | 必須 | デフォルト | 説明 |
|-----------|---|------|-----------|------|
| page | integer | No | 1 | ページ番号 (1以上) |
| pageSize | integer | No | 10 | 1ページの件数 (1-100) |
| status | string | No | all | フィルター (pending, completed, deleted, all) |
| search | string | No | - | タイトル検索文字列 |
| sortBy | string | No | createdAt | ソート項目 (createdAt, updatedAt, dueDate, title) |
| sortOrder | string | No | desc | ソート順 (asc, desc) |
| labelIds | array[string] | No | - | ラベルIDによるフィルター |

**レスポンス**:
```json
{
  "data": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "title": "プロジェクト会議の準備",
      "description": "来週の企画会議の資料を準備する",
      "status": "pending",
      "dueDate": "2024-01-15T10:00:00Z",
      "createdAt": "2024-01-01T09:00:00Z",
      "updatedAt": "2024-01-01T09:00:00Z",
      "completedAt": null,
      "labels": [
        {
          "id": "label1",
          "name": "仕事",
          "color": "#FF6B6B"
        }
      ],
      "files": [
        {
          "id": "file1",
          "fileName": "agenda.pdf",
          "fileSize": 1024,
          "contentType": "application/pdf",
          "uploadedAt": "2024-01-01T09:30:00Z"
        }
      ]
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

**ステータスコード**:
- `200 OK`: 成功
- `400 Bad Request`: 不正なパラメータ
- `401 Unauthorized`: 認証エラー
- `500 Internal Server Error`: サーバーエラー

### タスク詳細取得

**エンドポイント**: `GET /api/v1/tasks/{id}`

**説明**: 指定されたIDのタスク詳細を取得します。

**パスパラメータ**:
| パラメータ | 型 | 必須 | 説明 |
|-----------|---|------|------|
| id | string (UUID) | Yes | タスクID |

**レスポンス**:
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "title": "プロジェクト会議の準備",
  "description": "来週の企画会議の資料を準備する\n\n詳細な内容...",
  "status": "pending",
  "dueDate": "2024-01-15T10:00:00Z",
  "createdAt": "2024-01-01T09:00:00Z",
  "updatedAt": "2024-01-01T09:00:00Z",
  "completedAt": null,
  "labels": [
    {
      "id": "label1",
      "name": "仕事",
      "color": "#FF6B6B"
    }
  ],
  "files": [
    {
      "id": "file1",
      "fileName": "agenda.pdf",
      "fileSize": 1024,
      "contentType": "application/pdf",
      "blobUrl": "https://todoappblob.blob.core.windows.net/files/agenda.pdf",
      "uploadedAt": "2024-01-01T09:30:00Z"
    }
  ]
}
```

**ステータスコード**:
- `200 OK`: 成功
- `404 Not Found`: タスクが見つからない
- `401 Unauthorized`: 認証エラー
- `403 Forbidden`: アクセス権限なし

### タスク作成

**エンドポイント**: `POST /api/v1/tasks`

**説明**: 新しいタスクを作成します。

**リクエストボディ**:
```json
{
  "title": "新しいタスク",
  "description": "タスクの詳細説明",
  "dueDate": "2024-01-15T10:00:00Z",
  "labelIds": ["label1", "label2"]
}
```

**バリデーション**:
| フィールド | 必須 | 制約 |
|-----------|------|------|
| title | Yes | 1-200文字 |
| description | No | 最大2000文字 |
| dueDate | No | 未来の日時 |
| labelIds | No | 最大10個 |

**レスポンス**:
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "title": "新しいタスク",
  "description": "タスクの詳細説明",
  "status": "pending",
  "dueDate": "2024-01-15T10:00:00Z",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-01T10:00:00Z",
  "completedAt": null,
  "labels": [
    {
      "id": "label1",
      "name": "仕事",
      "color": "#FF6B6B"
    }
  ],
  "files": []
}
```

**ステータスコード**:
- `201 Created`: 作成成功
- `400 Bad Request`: バリデーションエラー
- `401 Unauthorized`: 認証エラー
- `422 Unprocessable Entity`: ビジネスルール違反

### タスク更新

**エンドポイント**: `PUT /api/v1/tasks/{id}`

**説明**: 既存のタスクを更新します。

**パスパラメータ**:
| パラメータ | 型 | 必須 | 説明 |
|-----------|---|------|------|
| id | string (UUID) | Yes | タスクID |

**リクエストボディ**:
```json
{
  "title": "更新されたタスク",
  "description": "更新されたタスクの詳細説明",
  "status": "completed",
  "dueDate": "2024-01-15T10:00:00Z",
  "labelIds": ["label1", "label3"]
}
```

**ステータスコード**:
- `200 OK`: 更新成功
- `400 Bad Request`: バリデーションエラー
- `404 Not Found`: タスクが見つからない
- `409 Conflict`: 同時更新競合

### タスク削除（論理削除）

**エンドポイント**: `DELETE /api/v1/tasks/{id}`

**説明**: タスクを論理削除します。

**パスパラメータ**:
| パラメータ | 型 | 必須 | 説明 |
|-----------|---|------|------|
| id | string (UUID) | Yes | タスクID |

**ステータスコード**:
- `204 No Content`: 削除成功
- `404 Not Found`: タスクが見つからない
- `401 Unauthorized`: 認証エラー

## Label Service API

### ラベル一覧取得

**エンドポイント**: `GET /api/v1/labels`

**説明**: ユーザーのラベル一覧を取得します。

**レスポンス**:
```json
{
  "data": [
    {
      "id": "label1",
      "name": "仕事",
      "color": "#FF6B6B",
      "taskCount": 15,
      "createdAt": "2024-01-01T09:00:00Z",
      "updatedAt": "2024-01-01T09:00:00Z"
    },
    {
      "id": "label2",
      "name": "個人",
      "color": "#4ECDC4",
      "taskCount": 8,
      "createdAt": "2024-01-01T09:00:00Z",
      "updatedAt": "2024-01-01T09:00:00Z"
    }
  ]
}
```

### ラベル作成

**エンドポイント**: `POST /api/v1/labels`

**リクエストボディ**:
```json
{
  "name": "新しいラベル",
  "color": "#6366F1"
}
```

**バリデーション**:
| フィールド | 必須 | 制約 |
|-----------|------|------|
| name | Yes | 1-50文字、ユーザー内でユニーク |
| color | No | HEX色コード形式 |

## File Service API

### ファイルアップロード

**エンドポイント**: `POST /api/v1/tasks/{taskId}/files`

**説明**: タスクにファイルを添付します。

**リクエスト**: `multipart/form-data`

**パスパラメータ**:
| パラメータ | 型 | 必須 | 説明 |
|-----------|---|------|------|
| taskId | string (UUID) | Yes | タスクID |

**フォームデータ**:
| フィールド | 型 | 必須 | 制約 |
|-----------|---|------|------|
| file | file | Yes | 最大10MB、対応形式: jpg, png, pdf, docx, xlsx |

**レスポンス**:
```json
{
  "id": "file1",
  "fileName": "document.pdf",
  "fileSize": 1024000,
  "contentType": "application/pdf",
  "blobUrl": "https://todoappblob.blob.core.windows.net/files/document.pdf",
  "uploadedAt": "2024-01-01T10:00:00Z"
}
```

**ステータスコード**:
- `201 Created`: アップロード成功
- `400 Bad Request`: ファイル形式エラー
- `413 Payload Too Large`: ファイルサイズ超過
- `404 Not Found`: タスクが見つからない

### ファイルダウンロード

**エンドポイント**: `GET /api/v1/files/{fileId}/download`

**説明**: ファイルをダウンロードします。

**パスパラメータ**:
| パラメータ | 型 | 必須 | 説明 |
|-----------|---|------|------|
| fileId | string (UUID) | Yes | ファイルID |

**レスポンス**: バイナリデータ

**ヘッダー**:
```http
Content-Type: application/pdf
Content-Disposition: attachment; filename="document.pdf"
Content-Length: 1024000
```

### ファイル削除

**エンドポイント**: `DELETE /api/v1/files/{fileId}`

**ステータスコード**:
- `204 No Content`: 削除成功
- `404 Not Found`: ファイルが見つからない

## エラーレスポンス

### 標準エラー形式

すべてのエラーレスポンスは以下の形式で返されます：

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "エラーメッセージ",
    "details": [
      {
        "field": "title",
        "message": "タイトルは必須です"
      }
    ]
  },
  "timestamp": "2024-01-01T10:00:00Z",
  "requestId": "req-123456",
  "path": "/api/v1/tasks"
}
```

### エラーコード一覧

| コード | HTTPステータス | 説明 |
|--------|---------------|------|
| VALIDATION_ERROR | 400 | バリデーションエラー |
| UNAUTHORIZED | 401 | 認証エラー |
| FORBIDDEN | 403 | 認可エラー |
| NOT_FOUND | 404 | リソースが見つからない |
| CONFLICT | 409 | リソースの競合 |
| UNPROCESSABLE_ENTITY | 422 | ビジネスルール違反 |
| INTERNAL_ERROR | 500 | 内部サーバーエラー |
| SERVICE_UNAVAILABLE | 503 | サービス利用不可 |

## レート制限

### 制限値
- **認証済みユーザー**: 1000リクエスト/分
- **未認証**: 100リクエスト/分

### レスポンスヘッダー
```http
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1640995200
```

## バージョニング

### URLバージョニング
APIバージョンはURLパスに含めます：
- `https://api.todoapp.example.com/api/v1/tasks`
- `https://api.todoapp.example.com/api/v2/tasks`

### 後方互換性
- マイナーバージョンアップは後方互換性を保持
- メジャーバージョンアップは破壊的変更を含む可能性
- 旧バージョンは最低12ヶ月間サポート

## API利用例

### cURL例

```bash
# タスク一覧取得
curl -X GET "https://api.todoapp.example.com/api/v1/tasks?page=1&pageSize=10" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -H "Accept: application/json"

# タスク作成
curl -X POST "https://api.todoapp.example.com/api/v1/tasks" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -H "Content-Type: application/json" \
     -d '{
       "title": "新しいタスク",
       "description": "説明",
       "dueDate": "2024-01-15T10:00:00Z"
     }'

# ファイルアップロード
curl -X POST "https://api.todoapp.example.com/api/v1/tasks/123e4567-e89b-12d3-a456-426614174000/files" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -F "file=@document.pdf"
```

### JavaScript例

```javascript
// Fetch APIを使用した例
const API_BASE = 'https://api.todoapp.example.com/api/v1';
const token = 'YOUR_JWT_TOKEN';

// タスク一覧取得
async function getTasks(page = 1, pageSize = 10) {
  const response = await fetch(`${API_BASE}/tasks?page=${page}&pageSize=${pageSize}`, {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Accept': 'application/json'
    }
  });
  
  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }
  
  return await response.json();
}

// タスク作成
async function createTask(taskData) {
  const response = await fetch(`${API_BASE}/tasks`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(taskData)
  });
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error.message);
  }
  
  return await response.json();
}
```

---

**作成日**: 2024年1月
**バージョン**: 1.0
**作成者**: API設計チーム