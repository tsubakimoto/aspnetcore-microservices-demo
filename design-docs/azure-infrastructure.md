# Azure インフラストラクチャ設計書

## 概要

ToDoアプリケーション マイクロサービスのAzureインフラストラクチャ設計書です。
高可用性、スケーラビリティ、セキュリティを重視したクラウドネイティブアーキテクチャを採用します。

## アーキテクチャ概要

### 全体構成図

```
┌─────────────────────────────────────────────────────────────────────┐
│                          Azure Front Door                          │
│                      (Global Load Balancer)                        │
└─────────────────────────┬───────────────────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────────────────┐
│                      Azure API Management                          │
│                    (API Gateway / Rate Limiting)                   │
└─────────────────────────┬───────────────────────────────────────────┘
                          │
              ┌───────────┼───────────┐
              │           │           │
┌─────────────▼──┐ ┌──────▼──────┐ ┌──▼─────────────┐
│   Task API     │ │  Label API  │ │   File API     │
│ (App Service)  │ │(App Service)│ │ (App Service)  │
│   East Japan   │ │ East Japan  │ │  East Japan    │
└─────────────┬──┘ └──────┬──────┘ └──┬─────────────┘
              │           │           │
┌─────────────▼──┐ ┌──────▼──────┐ ┌──▼─────────────┐
│   Azure SQL    │ │  Azure SQL  │ │  Azure Blob    │
│   Database     │ │  Database   │ │   Storage      │
│  (Task DB)     │ │ (Label DB)  │ │ (File Storage) │
└────────────────┘ └─────────────┘ └────────────────┘
```

### リージョン戦略

**プライマリリージョン**: Japan East
**セカンダリリージョン**: Japan West
**DR リージョン**: Southeast Asia

## Azure サービス構成

### 1. Compute Services

#### 1.1 Azure App Service

**Service Plan 設定**:
```yaml
App Service Plan:
  Name: todoapp-plan-prod
  SKU: P1V3 (Premium v3)
  OS: Windows
  Region: Japan East
  Scaling:
    Min Instances: 2
    Max Instances: 10
    Auto Scale Rules:
      - CPU > 70% for 5 minutes → Scale Out
      - CPU < 30% for 10 minutes → Scale In
      - Memory > 80% for 5 minutes → Scale Out
```

**Web Apps 構成**:

1. **Task API Service**
   ```yaml
   Name: todoapp-task-api-prod
   Runtime: .NET 9.0
   Always On: true
   Health Check: /health
   Configuration:
     - ASPNETCORE_ENVIRONMENT: Production
     - ApplicationInsights__ConnectionString: "<AI_CONNECTION_STRING>"
     - TaskDb__ConnectionString: "<TASK_DB_CONNECTION_STRING>"
   ```

2. **Label API Service**
   ```yaml
   Name: todoapp-label-api-prod
   Runtime: .NET 9.0
   Always On: true
   Health Check: /health
   Configuration:
     - ASPNETCORE_ENVIRONMENT: Production
     - ApplicationInsights__ConnectionString: "<AI_CONNECTION_STRING>"
     - LabelDb__ConnectionString: "<LABEL_DB_CONNECTION_STRING>"
   ```

3. **File API Service**
   ```yaml
   Name: todoapp-file-api-prod
   Runtime: .NET 9.0
   Always On: true
   Health Check: /health
   Configuration:
     - ASPNETCORE_ENVIRONMENT: Production
     - ApplicationInsights__ConnectionString: "<AI_CONNECTION_STRING>"
     - BlobStorage__ConnectionString: "<BLOB_CONNECTION_STRING>"
   ```

### 2. Database Services

#### 2.1 Azure SQL Database

**設定仕様**:
```yaml
SQL Server:
  Name: todoapp-sql-server-prod
  Region: Japan East
  Admin: sqladmin
  Active Directory Admin: todoapp-admin-group
  
Databases:
  - Name: TodoApp-Tasks
    SKU: S2 (Standard, 50 DTU)
    Max Size: 250 GB
    Backup Retention: 7 days
    
  - Name: TodoApp-Labels  
    SKU: S1 (Standard, 20 DTU)
    Max Size: 250 GB
    Backup Retention: 7 days
    
  - Name: TodoApp-Events
    SKU: S1 (Standard, 20 DTU)
    Max Size: 250 GB
    Backup Retention: 35 days
```

**セキュリティ設定**:
```yaml
Firewall Rules:
  - Allow Azure Services: true
  - App Service IPs: automatic
  - Developer IPs: specific ranges

Authentication:
  - SQL Authentication: enabled (for emergency)
  - Azure AD Authentication: enabled (primary)
  - Managed Identity: enabled

Encryption:
  - Transparent Data Encryption: enabled
  - Always Encrypted: sensitive columns
  - Certificate: Azure Key Vault managed
```

#### 2.2 レプリケーション設定

**Read Replica**:
```yaml
Primary: Japan East
Read Replicas:
  - Japan West (HA)
  - Southeast Asia (DR)

Failover Group:
  Name: todoapp-failover-group
  Primary: Japan East
  Secondary: Japan West
  Read-Write Endpoint:
    Failover Policy: Automatic
    Grace Period: 1 hour
  Read-Only Endpoint:
    Failover Policy: Automatic
```

### 3. Storage Services

#### 3.1 Azure Blob Storage

**設定仕様**:
```yaml
Storage Account:
  Name: todoappstorprod
  SKU: Standard_ZRS (Zone Redundant)
  Region: Japan East
  Performance: Standard
  Access Tier: Hot

Containers:
  - task-files:
      Access Level: Private
      Retention Policy: 7 years
      
  - task-files-thumbnails:
      Access Level: Private
      Retention Policy: 1 year
      
  - backups:
      Access Level: Private
      Retention Policy: 10 years

Lifecycle Management:
  - Cool Tier: after 30 days
  - Archive Tier: after 90 days
  - Delete: after 7 years
```

**CDN設定**:
```yaml
Azure CDN:
  Profile: todoapp-cdn-prod
  SKU: Standard_Microsoft
  Custom Domain: files.todoapp.com
  HTTPS: enabled
  Compression: enabled
  Caching Rules:
    - Images: 30 days
    - Documents: 7 days
```

### 4. Networking

#### 4.1 Virtual Network

```yaml
Virtual Network:
  Name: todoapp-vnet-prod
  Address Space: 10.0.0.0/16
  Region: Japan East

Subnets:
  - app-subnet:
      Address Range: 10.0.1.0/24
      Service Endpoints: Microsoft.Sql, Microsoft.Storage
      
  - gateway-subnet:
      Address Range: 10.0.2.0/24
      
  - private-endpoint-subnet:
      Address Range: 10.0.3.0/24

Network Security Groups:
  - app-nsg:
      Rules:
        - Allow HTTPS (443) from Internet
        - Allow HTTP (80) from Internet (redirect to HTTPS)
        - Deny all other inbound traffic
```

#### 4.2 Private Endpoints

```yaml
Private Endpoints:
  - SQL Database:
      Name: todoapp-sql-pe
      Subnet: private-endpoint-subnet
      Private DNS Zone: privatelink.database.windows.net
      
  - Blob Storage:
      Name: todoapp-blob-pe  
      Subnet: private-endpoint-subnet
      Private DNS Zone: privatelink.blob.core.windows.net
```

### 5. Security Services

#### 5.1 Azure Key Vault

```yaml
Key Vault:
  Name: todoapp-kv-prod
  SKU: Standard
  Region: Japan East
  
Access Policies:
  - App Services: Get, List (Secrets)
  - Deployment Service Principal: All permissions
  - Admin Group: All permissions

Secrets:
  - TaskDb-ConnectionString
  - LabelDb-ConnectionString  
  - BlobStorage-ConnectionString
  - ApplicationInsights-ConnectionString
  - JWT-SigningKey

Certificates:
  - todoapp-com-ssl
  - api-todoapp-com-ssl
```

#### 5.2 Azure Active Directory

```yaml
App Registrations:
  - TodoApp-API:
      Application ID URI: api://todoapp
      Scopes:
        - Tasks.Read
        - Tasks.Write
        - Labels.Read
        - Labels.Write
        - Files.Read
        - Files.Write
      
  - TodoApp-Client:
      Platform: SPA
      Redirect URIs: 
        - https://app.todoapp.com/callback
        - https://localhost:3000/callback (dev)

Groups:
  - TodoApp-Users: Standard users
  - TodoApp-Admins: Administrative users
  - TodoApp-Developers: Development team
```

#### 5.3 Managed Identity

```yaml
System Assigned Identities:
  - todoapp-task-api-prod
  - todoapp-label-api-prod
  - todoapp-file-api-prod

User Assigned Identity:
  - todoapp-services-identity

Permissions:
  - Key Vault: Get, List secrets
  - SQL Database: db_datareader, db_datawriter
  - Blob Storage: Storage Blob Data Contributor
```

### 6. Monitoring & Logging

#### 6.1 Application Insights

```yaml
Application Insights:
  Name: todoapp-ai-prod
  Region: Japan East
  Sampling Rate: 100% (可能な限り全データ収集)
  
Custom Metrics:
  - Task Creation Rate
  - Task Completion Rate
  - File Upload Success Rate
  - API Response Times
  - User Active Sessions

Alerts:
  - High Error Rate: > 5% for 5 minutes
  - Slow Response: > 2 seconds for 10 minutes
  - High CPU: > 80% for 15 minutes
  - Low Memory: < 200MB for 10 minutes
```

#### 6.2 Log Analytics Workspace

```yaml
Log Analytics:
  Name: todoapp-logs-prod
  Region: Japan East
  Retention: 90 days
  
Data Sources:
  - Application Insights
  - App Service Logs
  - SQL Database Logs
  - Blob Storage Logs
  - Network Security Group Logs

Kusto Queries:
  - Performance Monitoring
  - Error Analysis  
  - Security Auditing
  - Usage Analytics
```

### 7. API Management

#### 7.1 APIM 設定

```yaml
API Management:
  Name: todoapp-apim-prod
  SKU: Developer (Production では Standard 以上)
  Region: Japan East
  
APIs:
  - Task API:
      Base URL: /api/v1/tasks
      Backend: todoapp-task-api-prod.azurewebsites.net
      
  - Label API:
      Base URL: /api/v1/labels
      Backend: todoapp-label-api-prod.azurewebsites.net
      
  - File API:
      Base URL: /api/v1/files
      Backend: todoapp-file-api-prod.azurewebsites.net

Policies:
  - Rate Limiting:
      Authenticated Users: 1000 calls/minute
      Guest Users: 100 calls/minute
      
  - CORS:
      Allowed Origins: https://app.todoapp.com
      Allowed Methods: GET, POST, PUT, DELETE
      Allowed Headers: Authorization, Content-Type
      
  - JWT Validation:
      Issuer: https://login.microsoftonline.com/{tenant-id}/v2.0
      Audience: api://todoapp
```

### 8. DevOps & Deployment

#### 8.1 Azure DevOps

```yaml
Project: TodoApp-Microservices

Repositories:
  - todoapp-task-service
  - todoapp-label-service
  - todoapp-file-service
  - todoapp-infrastructure (ARM/Bicep templates)

Build Pipelines:
  - CI-Task-Service:
      Trigger: main, develop branches
      Agent: windows-latest
      Steps:
        - .NET restore, build, test
        - SonarQube analysis
        - Docker build & push to ACR
        - Publish artifacts
        
Release Pipelines:
  - CD-Production:
      Stages:
        - Development: Auto deploy on CI success
        - Staging: Auto deploy with approval
        - Production: Manual approval required
```

#### 8.2 Infrastructure as Code

**ARM Template (概要)**:
```json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "environment": {
      "type": "string",
      "allowedValues": ["dev", "staging", "prod"]
    },
    "location": {
      "type": "string",
      "defaultValue": "Japan East"
    }
  },
  "resources": [
    {
      "type": "Microsoft.Web/serverfarms",
      "apiVersion": "2022-03-01",
      "name": "[concat('todoapp-plan-', parameters('environment'))]",
      "location": "[parameters('location')]",
      "sku": {
        "name": "P1V3",
        "tier": "PremiumV3"
      }
    }
  ]
}
```

### 9. Backup & Disaster Recovery

#### 9.1 バックアップ戦略

```yaml
Database Backup:
  Automated Backup:
    Frequency: Daily
    Retention: 7 days (short-term)
    LTR: Weekly(4 weeks), Monthly(12 months), Yearly(10 years)
    
  Manual Backup:
    Before deployments: yes
    Before schema changes: yes
    
Blob Storage Backup:
  Soft Delete: 7 days
  Versioning: enabled
  Cross-Region Replication: Japan West
  
Configuration Backup:
  App Settings: Azure DevOps Library
  Infrastructure: ARM templates in Git
  Key Vault: Automatic backup included
```

#### 9.2 災害復旧計画

```yaml
RTO Target: 4 hours
RPO Target: 1 hour

DR Procedures:
  1. Database Failover:
     - Automatic failover to Japan West
     - Update connection strings
     - Verify data integrity
     
  2. App Service Deployment:
     - Deploy to Japan West region
     - Update DNS (Front Door)
     - Health check verification
     
  3. Storage Failover:
     - Switch to replicated storage
     - Update Blob endpoints
     - Verify file accessibility

Testing Schedule:
  - Monthly: Failover testing
  - Quarterly: Full DR drill
  - Annually: Business continuity exercise
```

### 10. Cost Optimization

#### 10.1 リソース最適化

```yaml
App Service:
  Development: B1 (Basic)
  Staging: S1 (Standard)
  Production: P1V3 (Premium) with auto-scaling

SQL Database:
  Development: S0 (10 DTU)
  Staging: S1 (20 DTU)  
  Production: S2 (50 DTU) with auto-scaling

Storage:
  Lifecycle policies: enabled
  Reserved capacity: 1 year commitment
  Cool/Archive tiers: automatic transition

Reserved Instances:
  App Service: 1 year reservation (30% savings)
  SQL Database: 1 year reservation (20% savings)
```

#### 10.2 モニタリング & アラート

```yaml
Cost Alerts:
  - Monthly budget: $500 USD
  - Alert at: 80% of budget
  - Action: Email to admin team

Resource Optimization:
  - Azure Advisor recommendations: weekly review
  - Unused resources: monthly audit
  - Right-sizing: quarterly review

Cost Analysis:
  - Resource group breakdown: monthly
  - Service category analysis: monthly
  - Trend analysis: quarterly
```

### 11. セキュリティ設定

#### 11.1 Network Security

```yaml
DDoS Protection:
  Standard Plan: enabled on VNet
  Alert threshold: 1000 packets/sec

Web Application Firewall:
  Mode: Prevention
  Rule Set: OWASP 3.1
  Custom Rules:
    - Block known malicious IPs
    - Rate limit per IP: 100 req/min

SSL/TLS:
  Minimum Version: TLS 1.2
  Cipher Suites: Strong encryption only
  HSTS: enabled with 1 year max-age
```

#### 11.2 データ保護

```yaml
Encryption at Rest:
  SQL Database: TDE with customer-managed keys
  Blob Storage: Microsoft-managed keys
  Key Vault: HSM-backed keys for production

Encryption in Transit:
  All communications: HTTPS/TLS 1.2+
  Internal services: mTLS when possible
  Database connections: encrypted

Data Classification:
  Personal Data: encrypted + access logging
  Financial Data: not applicable
  System Data: standard encryption
```

### 12. 運用監視

#### 12.1 ヘルスチェック

```yaml
App Service Health Checks:
  Path: /health
  Interval: 60 seconds
  Timeout: 30 seconds
  Unhealthy threshold: 3 failures

SQL Database Monitoring:
  DTU utilization: < 80%
  Storage usage: < 90%
  Connection count: < 150 (per database)
  Query performance: top 10 slow queries

Blob Storage Monitoring:
  Availability: > 99.9%
  Success rate: > 99%
  End-to-end latency: < 1000ms
```

#### 12.2 アラート設定

```yaml
Critical Alerts (24/7):
  - Service down: any health check failure
  - Database offline: connection failures
  - High error rate: > 5% in 5 minutes

Warning Alerts (business hours):
  - High resource usage: > 80% for 15 minutes
  - Slow response times: > 2 seconds avg
  - Storage quota: > 90% usage

Info Alerts (daily summary):
  - Performance summary
  - Cost summary
  - Security summary
```

---

**作成日**: 2024年1月
**バージョン**: 1.0
**作成者**: インフラストラクチャ設計チーム